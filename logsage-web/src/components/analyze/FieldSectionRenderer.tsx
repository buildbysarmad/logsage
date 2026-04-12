import type { FieldSection } from '@/lib/types';
import DisplayFieldRenderer from './DisplayFieldRenderer';

interface Props {
  section: FieldSection;
}

export default function FieldSectionRenderer({ section }: Props) {
  // Filter out fields with no value
  const visibleFields = section.fields.filter(f => f.value !== null && f.value !== undefined);

  if (visibleFields.length === 0) {
    return null;
  }

  // Group fields by importance for rendering
  const primaryFields = visibleFields.filter(f => f.importance === 'Primary');
  const secondaryFields = visibleFields.filter(f => f.importance === 'Secondary');

  return (
    <div>
      {/* Render primary fields inline */}
      {primaryFields.length > 0 && (
        <div className="flex flex-wrap gap-2 items-baseline">
          {primaryFields.map((field) => (
            <DisplayFieldRenderer key={field.key} field={field} />
          ))}
        </div>
      )}

      {/* Render secondary fields (URLs, paths, etc.) as block elements */}
      {secondaryFields.map((field) => (
        <DisplayFieldRenderer key={field.key} field={field} />
      ))}
    </div>
  );
}
