export interface LogEntry {
  timestamp: string | null;
  level: 'Trace' | 'Debug' | 'Info' | 'Warning' | 'Error' | 'Fatal';
  message: string;
  stackTrace: string | null;
  source: string | null;
  exceptionType: string | null;
  lineNumber: number;
  rawLine: string;
}

export interface ErrorGroup {
  groupKey: string;
  representativeMessage: string;
  level: LogEntry['level'];
  count: number;
  firstSeen: string | null;
  lastSeen: string | null;
  exceptionType: string | null;
  source: string | null;
  entries: LogEntry[];
}

export interface AiGroupAnalysis {
  groupKey: string;
  severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  rootCause: string;
  suggestedFix: string;
}

export interface AnalysisResult {
  detectedFormat: string;
  totalLines: number;
  parsedEntries: number;
  errorCount: number;
  warningCount: number;
  logDuration: string | null;
  wasTruncated: boolean;
  errorGroups: ErrorGroup[];
  aiAnalysis: AiGroupAnalysis[];
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface User {
  id: string;
  email: string;
  plan: 'free' | 'pro' | 'team';
  createdAt?: string;
}

export interface SessionSummary {
  id: string;
  detectedFormat: string;
  totalLines: number;
  errorCount: number;
  warningCount: number;
  createdAt: string;
}
