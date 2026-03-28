import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  analyzeApi: {
    text: vi.fn(),
    file: vi.fn(),
  },
}));

describe('Analyze page', () => {
  it('renders the upload tab by default', async () => {
    const { default: AnalyzePage } = await import('@/app/(app)/analyze/page');
    render(<AnalyzePage />);
    expect(screen.getByText('File upload')).toBeDefined();
    expect(screen.getByText('Paste text')).toBeDefined();
  });
});
