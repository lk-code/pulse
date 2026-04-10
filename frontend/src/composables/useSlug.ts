export function toSlug(normalizedName: string, id: number): string {
  return `${normalizedName}-${id}`
}

export function parseSlug(slug: string): { id: number; normalizedName: string } {
  const lastDash = slug.lastIndexOf('-')
  return {
    normalizedName: slug.substring(0, lastDash),
    id: parseInt(slug.substring(lastDash + 1), 10)
  }
}
