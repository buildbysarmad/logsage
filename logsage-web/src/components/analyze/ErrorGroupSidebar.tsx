import type { ErrorGroup } from '@/lib/types';

const levelDot: Record<string, string> = {
  Fatal: 'bg-red-500',
  Error: 'bg-red-400',
  Warning: 'bg-yellow-400',
  Info: 'bg-blue-400',
  Debug: 'bg-gray-400',
  Trace: 'bg-gray-600',
};

const levelBadge: Record<string, string> = {
  Fatal: 'bg-red-900 text-red-300',
  Error: 'bg-red-900 text-red-300',
  Warning: 'bg-yellow-900 text-yellow-300',
  Info: 'bg-blue-900 text-blue-300',
  Debug: 'bg-gray-800 text-gray-400',
  Trace: 'bg-gray-800 text-gray-500',
};

interface Props {
  groups: ErrorGroup[];
  selected: ErrorGroup | null;
  onSelect: (g: ErrorGroup) => void;
}

export default function ErrorGroupSidebar({ groups, selected, onSelect }: Props) {
  if (groups.length === 0) {
    return (
      <aside className="w-56 shrink-0 border-r border-gray-800 bg-gray-900 flex items-center justify-center">
        <p className="text-xs text-gray-500 text-center px-4">
          No errors or warnings found in this log file.
        </p>
      </aside>
    );
  }

  return (
    <aside className="w-56 shrink-0 border-r border-gray-800 bg-gray-900 flex flex-col overflow-y-auto">
      <div className="px-3 py-2.5 border-b border-gray-800 shrink-0">
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
          {groups.length} error group{groups.length !== 1 ? 's' : ''}
        </p>
      </div>
      {groups.map((group) => (
        <button
          key={group.groupKey}
          onClick={() => onSelect(group)}
          className={`w-full text-left px-3 py-3 border-b border-gray-800
                      transition-colors hover:bg-gray-800 ${
                        selected?.groupKey === group.groupKey
                          ? 'bg-emerald-950 border-l-2 border-l-emerald-500'
                          : ''
                      }`}
        >
          <div className="flex items-start gap-2">
            <span
              className={`mt-1 w-2 h-2 rounded-full shrink-0 ${
                levelDot[group.level] ?? 'bg-gray-500'
              }`}
            />
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-gray-200 truncate">
                {group.exceptionType ?? group.representativeMessage}
              </p>
              {group.firstSeen && (
                <p className="text-xs text-gray-500 mt-0.5">
                  {group.firstSeen.slice(11, 19)}
                </p>
              )}
            </div>
            <span className={`text-xs font-semibold px-1.5 py-0.5 rounded shrink-0 ${
              levelBadge[group.level] ?? 'bg-gray-800 text-gray-400'
            }`}>
              {group.count}
            </span>
          </div>
        </button>
      ))}
    </aside>
  );
}
