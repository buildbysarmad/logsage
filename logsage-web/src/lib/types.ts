export type FieldType = 'Text' | 'Number' | 'Duration' | 'Timestamp' | 'Url' | 'StackTrace' | 'Json' | 'ExceptionType';
export type FieldImportance = 'Primary' | 'Secondary' | 'Debug';

export interface DisplayField {
  key: string;
  displayName: string;
  value: unknown;
  type: FieldType;
  importance: FieldImportance;
  hints?: Record<string, unknown>;
}

export interface FieldSection {
  sectionName: string;
  displayOrder: number;
  fields: DisplayField[];
}

export interface LogEntry {
  timestamp: string | null;
  level: 'Trace' | 'Debug' | 'Info' | 'Warning' | 'Error' | 'Fatal';
  message: string;
  stackTrace: string | null;
  source: string | null;
  exceptionType: string | null;
  lineNumber: number;
  rawLine: string;
  parserType: string;

  // Structured field sections (new architecture)
  fieldSections: FieldSection[];

  // Legacy flat fields (backward compatibility - deprecated)
  structuredFields?: Record<string, unknown>;
  requestId?: string | null;
  requestPath?: string | null;
  connectionId?: string | null;
  statusCode?: number | null;
  sourceContext?: string | null;

  // Parse metadata
  parseError?: boolean;
  parseErrorMessage?: string | null;
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
  infoCount: number;
  debugCount: number;
  parseErrorCount: number;
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
