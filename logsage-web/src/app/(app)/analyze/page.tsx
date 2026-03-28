'use client';

import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { analyzeApi } from '@/lib/api';
import type { AnalysisResult, ErrorGroup } from '@/lib/types';
import ErrorGroupSidebar from '@/components/analyze/ErrorGroupSidebar';
import LogDetailPanel from '@/components/analyze/LogDetailPanel';
import AiAnalysisPanel from '@/components/analyze/AiAnalysisPanel';

export default function AnalyzePage() {
  const [result, setResult] = useState<AnalysisResult | null>(null);
  const [selected, setSelected] = useState<ErrorGroup | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<'upload' | 'paste'>('upload');
  const [paste, setPaste] = useState('');

  const analyze = async (rawLog: string) => {
    setLoading(true);
    setError(null);
    try {
      const { data } = await analyzeApi.text(rawLog);
      setResult(data);
      setSelected(data.errorGroups[0] ?? null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Analysis failed';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const onDrop = useCallback(async (files: File[]) => {
    if (!files[0]) return;
    setLoading(true);
    setError(null);
    try {
      const { data } = await analyzeApi.file(files[0]);
      setResult(data);
      setSelected(data.errorGroups[0] ?? null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Upload failed';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'text/plain': ['.log', '.txt'] },
    maxFiles: 1,
    disabled: loading,
  });

  const aiForSelected = result?.aiAnalysis.find(
    (a) => a.groupKey === selected?.groupKey
  );

  if (!result) {
    return (
      <div className="min-h-screen bg-gray-950 flex flex-col items-center justify-center p-8">
        <h1 className="text-3xl font-semibold text-white mb-2">
          log<span className="text-emerald-400">lens</span>
        </h1>
        <p className="text-gray-400 mb-8 text-sm">
          Paste your logs. Know what broke. In 10 seconds.
        </p>

        <div className="flex gap-2 mb-6">
          {(['upload', 'paste'] as const).map((t) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                tab === t
                  ? 'bg-emerald-600 text-white'
                  : 'bg-gray-800 text-gray-400 hover:text-white'
              }`}
            >
              {t === 'upload' ? 'File upload' : 'Paste text'}
            </button>
          ))}
        </div>

        {tab === 'upload' ? (
          <div
            {...getRootProps()}
            className={`w-full max-w-xl border-2 border-dashed rounded-xl p-12 text-center cursor-pointer transition-colors ${
              isDragActive
                ? 'border-emerald-400 bg-emerald-950'
                : 'border-gray-700 hover:border-gray-500'
            }`}
          >
            <input {...getInputProps()} />
            <p className="text-gray-400 text-sm">
              {loading
                ? 'Analyzing...'
                : isDragActive
                ? 'Drop your log file here...'
                : 'Drag & drop a .log or .txt file, or click to browse'}
            </p>
          </div>
        ) : (
          <div className="w-full max-w-xl">
            <textarea
              value={paste}
              onChange={(e) => setPaste(e.target.value)}
              placeholder="Paste your log content here..."
              className="w-full h-48 bg-gray-900 text-gray-200 text-sm font-mono
                         border border-gray-700 rounded-xl p-4 resize-none
                         focus:outline-none focus:border-emerald-500"
            />
            <button
              onClick={() => analyze(paste)}
              disabled={!paste.trim() || loading}
              className="mt-3 w-full bg-emerald-600 hover:bg-emerald-500
                         disabled:opacity-50 disabled:cursor-not-allowed
                         text-white font-medium py-2.5 rounded-lg transition-colors"
            >
              {loading ? 'Analyzing...' : 'Analyze logs'}
            </button>
          </div>
        )}

        {error && <p className="mt-4 text-red-400 text-sm">{error}</p>}

        <p className="mt-8 text-xs text-gray-600">
          Free tier: 3 analyses/day · 500 lines max · No account needed
        </p>
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-gray-950 text-gray-200 overflow-hidden">
      {/* Top bar */}
      <div className="flex items-center justify-between px-4 py-2.5 border-b border-gray-800 bg-gray-900 shrink-0">
        <span className="font-semibold text-white text-sm">
          log<span className="text-emerald-400">lens</span>
        </span>
        <div className="flex items-center gap-3 text-xs text-gray-400">
          <span className="bg-gray-800 px-2 py-0.5 rounded">{result.detectedFormat}</span>
          <span>{result.totalLines.toLocaleString()} lines</span>
          <span className="text-red-400">{result.errorCount} errors</span>
          <span className="text-yellow-400">{result.warningCount} warnings</span>
          {result.wasTruncated && (
            <span className="text-orange-400">truncated to 500 lines</span>
          )}
        </div>
        <button
          onClick={() => { setResult(null); setSelected(null); }}
          className="text-xs text-gray-500 hover:text-white transition-colors"
        >
          New analysis
        </button>
      </div>

      {/* 3-panel layout */}
      <div className="flex flex-1 overflow-hidden">
        <ErrorGroupSidebar
          groups={result.errorGroups}
          selected={selected}
          onSelect={setSelected}
        />
        <LogDetailPanel group={selected} />
        <AiAnalysisPanel analysis={aiForSelected ?? null} group={selected} />
      </div>
    </div>
  );
}
