import type { DisplayField } from '@/lib/types';

interface Props {
  field: DisplayField;
}

export default function DisplayFieldRenderer({ field }: Props) {
  if (field.value === null || field.value === undefined) {
    return null;
  }

  const baseClass = 'text-[10px]';

  switch (field.type) {
    case 'StackTrace':
      return (
        <pre className="mt-1.5 text-xs opacity-60 whitespace-pre-wrap overflow-x-auto scrollbar-thin">
          {String(field.value)}
        </pre>
      );

    case 'Url':
      return (
        <div className="mt-1 text-[10px] opacity-50">
          {String(field.value)}
        </div>
      );

    case 'Number': {
      const numValue = Number(field.value);
      const colorThreshold = field.hints?.colorThreshold as number | undefined;
      const shouldHighlight = colorThreshold !== undefined && numValue >= colorThreshold;

      return (
        <span className={`${baseClass} ${shouldHighlight ? 'text-red-400' : 'opacity-60'}`}>
          {String(field.value)}
        </span>
      );
    }

    case 'ExceptionType':
      return (
        <span className="font-semibold text-red-300">
          {String(field.value)}
        </span>
      );

    default:
      return (
        <span className={`${baseClass} opacity-60`}>
          {field.displayName}: {String(field.value)}
        </span>
      );
  }
}
