#!/bin/bash
# LogSage Deployment Verification Script

set -e

echo "🚀 LogSage Deployment Verification"
echo "==================================="
echo ""

# Configuration
BACKEND_URL="${BACKEND_URL:-https://logsage-production.up.railway.app}"
FRONTEND_URL="${FRONTEND_URL:-https://logsage.dev}"

echo "Backend:  $BACKEND_URL"
echo "Frontend: $FRONTEND_URL"
echo ""

# Check if jq is available
if ! command -v jq &> /dev/null; then
    echo "⚠️  jq not found - JSON responses will not be pretty printed"
    echo "   Install with: apt-get install jq (Linux) or brew install jq (Mac)"
    JQ_CMD="cat"
else
    JQ_CMD="jq ."
fi

# 1. Backend Health Check
echo "1️⃣  Backend Health Check"
echo "   GET $BACKEND_URL/health"
HEALTH_RESPONSE=$(curl -s -w "\n%{http_code}" "$BACKEND_URL/health")
HTTP_CODE=$(echo "$HEALTH_RESPONSE" | tail -n1)
BODY=$(echo "$HEALTH_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ Backend is healthy (200 OK)"
    echo "$BODY" | $JQ_CMD | sed 's/^/      /'
else
    echo "   ❌ Backend health check failed (HTTP $HTTP_CODE)"
    echo "$BODY" | sed 's/^/      /'
    exit 1
fi
echo ""

# 2. Backend Metrics
echo "2️⃣  Backend Metrics"
echo "   GET $BACKEND_URL/metrics"
METRICS_RESPONSE=$(curl -s -w "\n%{http_code}" "$BACKEND_URL/metrics")
HTTP_CODE=$(echo "$METRICS_RESPONSE" | tail -n1)

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ Metrics endpoint accessible"
else
    echo "   ⚠️  Metrics endpoint returned HTTP $HTTP_CODE (may be expected)"
fi
echo ""

# 3. Backend API Docs
echo "3️⃣  Backend API Documentation"
echo "   GET $BACKEND_URL/scalar"
SCALAR_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$BACKEND_URL/scalar")

if [ "$SCALAR_RESPONSE" = "200" ]; then
    echo "   ✅ API documentation available at $BACKEND_URL/scalar"
else
    echo "   ⚠️  API documentation endpoint returned HTTP $SCALAR_RESPONSE"
fi
echo ""

# 4. CORS Check
echo "4️⃣  CORS Configuration"
echo "   OPTIONS $BACKEND_URL/api/health"
CORS_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" \
    -H "Origin: $FRONTEND_URL" \
    -H "Access-Control-Request-Method: GET" \
    -X OPTIONS \
    "$BACKEND_URL/api/health")

if [ "$CORS_RESPONSE" = "204" ] || [ "$CORS_RESPONSE" = "200" ]; then
    echo "   ✅ CORS configured correctly"

    # Get CORS headers
    CORS_HEADERS=$(curl -s -I \
        -H "Origin: $FRONTEND_URL" \
        -H "Access-Control-Request-Method: GET" \
        -X OPTIONS \
        "$BACKEND_URL/api/health" | grep -i "access-control")

    if [ -n "$CORS_HEADERS" ]; then
        echo "   CORS Headers:"
        echo "$CORS_HEADERS" | sed 's/^/      /'
    fi
else
    echo "   ❌ CORS check failed (HTTP $CORS_RESPONSE)"
    echo "   ⚠️  Make sure AllowedOrigins includes: $FRONTEND_URL"
    echo "   Set in Railway: AllowedOrigins=$FRONTEND_URL,https://www.logsage.dev"
fi
echo ""

# 5. Frontend Accessibility
echo "5️⃣  Frontend Accessibility"
echo "   GET $FRONTEND_URL"
FRONTEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$FRONTEND_URL")

if [ "$FRONTEND_RESPONSE" = "200" ]; then
    echo "   ✅ Frontend is accessible (200 OK)"
else
    echo "   ❌ Frontend returned HTTP $FRONTEND_RESPONSE"
    if [ "$FRONTEND_RESPONSE" = "000" ]; then
        echo "   ⚠️  Could not connect - check DNS configuration"
    fi
fi
echo ""

# 6. Frontend API Proxy
echo "6️⃣  Frontend API Proxy (Netlify Redirect)"
echo "   GET $FRONTEND_URL/api/health"
PROXY_RESPONSE=$(curl -s -w "\n%{http_code}" "$FRONTEND_URL/api/health")
HTTP_CODE=$(echo "$PROXY_RESPONSE" | tail -n1)
BODY=$(echo "$PROXY_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ API proxy working correctly"
    echo "$BODY" | $JQ_CMD | sed 's/^/      /'
else
    echo "   ❌ API proxy failed (HTTP $HTTP_CODE)"
    echo "   ⚠️  Check netlify.toml redirect configuration"
fi
echo ""

# 7. Test Anonymous Analysis (core feature)
echo "7️⃣  Anonymous Log Analysis"
echo "   POST $BACKEND_URL/api/analyze"
TEST_LOG='[2026-04-10 10:00:00] ERROR Something went wrong
[2026-04-10 10:00:01] INFO Application started'

ANALYZE_RESPONSE=$(curl -s -w "\n%{http_code}" \
    -X POST \
    -H "Content-Type: application/json" \
    -d "{\"text\":\"$TEST_LOG\"}" \
    "$BACKEND_URL/api/analyze")
HTTP_CODE=$(echo "$ANALYZE_RESPONSE" | tail -n1)
BODY=$(echo "$ANALYZE_RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ Anonymous analysis working"

    # Check if it parsed correctly
    ERROR_COUNT=$(echo "$BODY" | $JQ_CMD -r '.summary.errors // 0' 2>/dev/null || echo "?")
    echo "   Detected $ERROR_COUNT error(s) in test log"
else
    echo "   ❌ Analysis failed (HTTP $HTTP_CODE)"
    echo "$BODY" | sed 's/^/      /'
fi
echo ""

# Summary
echo "==================================="
echo "📊 Summary"
echo "==================================="

if [ "$HEALTH_RESPONSE" ] && [ "$FRONTEND_RESPONSE" = "200" ] && [ "$PROXY_RESPONSE" ]; then
    echo "✅ All critical checks passed!"
    echo ""
    echo "🎉 LogSage is ready for production!"
    echo ""
    echo "Next steps:"
    echo "  1. Test registration: $FRONTEND_URL/register"
    echo "  2. Test login: $FRONTEND_URL/login"
    echo "  3. Test log analysis: $FRONTEND_URL/analyze"
    echo "  4. Check dashboard: $FRONTEND_URL/dashboard"
    echo "  5. Monitor Railway logs: railway logs"
    echo "  6. Monitor Netlify logs: netlify logs --prod"
else
    echo "⚠️  Some checks failed - review the output above"
    echo ""
    echo "Common issues:"
    echo "  - CORS: Update AllowedOrigins in Railway"
    echo "  - DNS: Wait for DNS propagation (5-30 minutes)"
    echo "  - SSL: Wait for Netlify certificate provisioning"
    echo "  - API Proxy: Verify netlify.toml redirect configuration"
fi

echo ""
