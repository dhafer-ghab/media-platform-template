// Verso gallery dataset — ported from the original Claude design export.
// Backgrounds are procedurally generated CSS gradients (no real images).

export const palette = {
  ink: '#221F1A',
  char: '#2B2A28',
  oat: '#E7DEC9',
  clay: '#B5563A',
  ochre: '#C99A3F',
  sage: '#8C9A78',
  slate: '#586A72',
  plum: '#6E4B5E',
  sand: '#D9C7A6',
  teal: '#356A64',
  oxblood: '#7A2E2A',
  sky: '#A9C2CC',
  rose: '#C98A7E',
  forest: '#2F4538',
  mustard: '#D4A82C',
  stone: '#9B9384',
  cobalt: '#33527A',
  blush: '#E8D5CC',
  umber: '#4A3B2E',
  bone: '#F0E9DA',
  noir: '#1A1714',
} as const;

type PaletteKey = keyof typeof palette;

type RawArtist = readonly [string, string, string, string, PaletteKey, string];
type RawWork = readonly [
  number,
  string,
  number,
  string,
  string,
  string,
  BgPattern,
  string,
];

type BgPattern =
  | 'field'
  | 'duo'
  | 'split'
  | 'horizon'
  | 'sun'
  | 'glow'
  | 'bars'
  | 'vbars'
  | 'corner';

const rawArtists: RawArtist[] = [
  ['Mira Okafor', 'Lagos, Nigeria', 'Painting · Collage', 'Builds dense, layered surfaces where torn paper and pigment compete for the same ground.', 'clay', '2014'],
  ['Johan Vesterberg', 'Stockholm, Sweden', 'Photography', 'Photographs northern light and stone with a patience that borders on devotion.', 'slate', '2009'],
  ['Renata Salgado', 'São Paulo, Brazil', 'Sculpture · Installation', 'Makes weight look weightless — balancing bronze, concrete and colour against gravity.', 'stone', '2012'],
  ['Aiko Tanaka', 'Kyoto, Japan', 'Printmaking · Drawing', 'Cuts woodblocks of rain, tide and line — quiet repetitions that accumulate into weather.', 'sky', '2016'],
  ['Daniel Mercer', 'Glasgow, Scotland', 'Painting', 'Paints the city after dark: lit windows, wet streets, the interior life of buildings.', 'umber', '2008'],
  ['Yara Haddad', 'Beirut, Lebanon', 'Textile · Mixed Media', 'Dyes and stitches maps of memory and home into cotton, wool and indigo.', 'oxblood', '2015'],
  ['Tomas Brandt', 'Berlin, Germany', 'Digital · Generative', 'Writes systems that draw — fields and lattices where code behaves like pigment.', 'cobalt', '2018'],
  ['Lucia Ferraro', 'Naples, Italy', 'Photography · Painting', 'Moves between lens and brush, chasing the same Mediterranean light through both.', 'rose', '2011'],
  ['Noah Adeyemi', 'Brooklyn, USA', 'Drawing · Painting', 'Draws the figure with charcoal and acrylic — portraits of presence and congregation.', 'char', '2017'],
];

const rawWorks: RawWork[] = [
  [0, 'Market at Noon', 2024, 'Painting', 'Oil and paper on linen', 'Abstract,Color Field', 'glow', 'clay,ochre'],
  [0, 'Untitled (Red Ground)', 2023, 'Painting', 'Oil on canvas', 'Abstract,Color Field,Minimal', 'field', 'oxblood'],
  [0, 'Crossings', 2024, 'Collage', 'Mixed media on board', 'Geometric,Abstract', 'bars', 'clay,oat'],
  [0, 'Harmattan', 2022, 'Painting', 'Acrylic on canvas', 'Landscape,Abstract', 'horizon', 'ochre,sand,clay'],
  [1, 'North Light I', 2023, 'Photography', 'Archival pigment print', 'Landscape,Minimal,Monochrome', 'horizon', 'slate,sky,bone'],
  [1, 'Quarry', 2022, 'Photography', 'Silver gelatin print', 'Monochrome,Minimal', 'duo', 'char,stone'],
  [1, 'Field Study', 2024, 'Photography', 'Archival pigment print', 'Landscape,Minimal', 'horizon', 'sage,bone,stone'],
  [1, 'Ice Sheet', 2023, 'Photography', 'Archival pigment print', 'Minimal,Monochrome', 'field', 'sky'],
  [2, 'Balance no.7', 2024, 'Sculpture', 'Bronze and steel', 'Geometric,Abstract', 'split', 'char,sand'],
  [2, 'Soft Architecture', 2023, 'Sculpture', 'Cast concrete', 'Minimal,Geometric', 'field', 'stone'],
  [2, 'Counterweight', 2022, 'Sculpture', 'Carrara marble', 'Minimal,Organic', 'glow', 'bone,stone'],
  [2, 'Trópico', 2024, 'Installation', 'Steel and pigment', 'Color Field,Geometric', 'corner', 'forest,mustard'],
  [3, 'Rainband', 2024, 'Printmaking', 'Woodblock print', 'Minimal,Landscape', 'vbars', 'sky,bone'],
  [3, 'Hundred Lines', 2023, 'Drawing', 'Ink on paper', 'Minimal,Monochrome,Geometric', 'vbars', 'char,bone'],
  [3, 'Plum Rain', 2022, 'Printmaking', 'Woodblock print', 'Organic,Minimal', 'glow', 'plum,blush'],
  [3, 'Tide Table', 2024, 'Printmaking', 'Screenprint', 'Geometric,Color Field', 'bars', 'teal,oat'],
  [4, 'Tenement Window', 2023, 'Painting', 'Oil on board', 'Figurative,Still Life', 'duo', 'slate,umber'],
  [4, 'Clyde', 2024, 'Painting', 'Oil on canvas', 'Landscape,Abstract', 'horizon', 'slate,stone,char'],
  [4, 'Interior with Lamp', 2022, 'Painting', 'Oil on canvas', 'Figurative,Still Life', 'glow', 'umber,mustard'],
  [4, 'Nocturne', 2024, 'Painting', 'Oil on linen', 'Abstract,Color Field,Monochrome', 'field', 'noir'],
  [5, 'Threshold', 2024, 'Textile', 'Hand-dyed cotton', 'Geometric,Color Field', 'bars', 'oxblood,sand'],
  [5, 'Mending I', 2023, 'Textile', 'Wool and linen', 'Organic,Abstract', 'corner', 'rose,oat'],
  [5, 'Cartography', 2022, 'Mixed Media', 'Thread on canvas', 'Abstract,Geometric', 'split', 'teal,sand'],
  [5, 'Domestic Map', 2024, 'Textile', 'Cotton and indigo', 'Geometric,Color Field,Monochrome', 'vbars', 'cobalt,bone'],
  [6, 'Field 0x9', 2024, 'Digital', 'Generative print', 'Abstract,Geometric,Color Field', 'corner', 'cobalt,sky'],
  [6, 'Drift', 2023, 'Digital', 'Generative still', 'Abstract,Minimal', 'glow', 'plum,cobalt'],
  [6, 'Lattice', 2024, 'Digital', 'Generative print', 'Geometric,Minimal', 'bars', 'char,slate'],
  [6, 'Solar', 2023, 'Digital', 'Generative print', 'Color Field,Abstract', 'sun', 'ink,mustard'],
  [7, 'Bay, Evening', 2024, 'Photography', 'Archival pigment print', 'Landscape,Color Field', 'horizon', 'cobalt,rose,sand'],
  [7, 'Still Life with Lemons', 2023, 'Painting', 'Oil on panel', 'Still Life,Figurative', 'glow', 'mustard,forest'],
  [7, 'Vesuvio', 2022, 'Photography', 'Archival pigment print', 'Landscape,Minimal', 'horizon', 'plum,rose,stone'],
  [7, 'Window, Posillipo', 2024, 'Painting', 'Oil on canvas', 'Figurative,Color Field', 'split', 'sky,blush'],
  [8, 'Sitter I', 2024, 'Drawing', 'Charcoal on paper', 'Portrait,Figurative,Monochrome', 'glow', 'char,stone'],
  [8, 'Two Figures', 2023, 'Painting', 'Acrylic on canvas', 'Figurative,Color Field', 'split', 'clay,teal'],
  [8, 'Self, Morning', 2022, 'Drawing', 'Graphite on paper', 'Portrait,Monochrome,Minimal', 'field', 'stone'],
  [8, 'Congregation', 2024, 'Painting', 'Acrylic on canvas', 'Figurative,Abstract', 'corner', 'oxblood,ochre'],
];

const aspectRatios = [0.8, 1.3, 1, 0.78, 1.5, 0.92];

function genBg(pattern: BgPattern, colors: string[]): string {
  const [c0, c1, c2] = colors;
  switch (pattern) {
    case 'field':
      return c0;
    case 'duo':
      return `linear-gradient(150deg, ${c0}, ${c1 ?? c0})`;
    case 'split':
      return `linear-gradient(100deg, ${c0} 0 50%, ${c1 ?? c0} 50% 100%)`;
    case 'horizon':
      return `linear-gradient(${c0} 0 40%, ${c1 ?? c0} 40% 64%, ${c2 ?? c1 ?? c0} 64% 100%)`;
    case 'sun':
      return `radial-gradient(circle at 50% 62%, ${c1 ?? c0} 0 19%, ${c0} 19%)`;
    case 'glow':
      return `radial-gradient(125% 92% at 28% 22%, ${c1 ?? c0}, ${c0})`;
    case 'bars':
      return `repeating-linear-gradient(90deg, ${c0} 0 12%, ${c1 ?? c0} 12% 24%)`;
    case 'vbars':
      return `repeating-linear-gradient(0deg, ${c0} 0 8%, ${c1 ?? c0} 8% 16%)`;
    case 'corner':
      return `conic-gradient(from 210deg at 78% 22%, ${c1 ?? c0}, ${c0}, ${c1 ?? c0})`;
    default:
      return c0;
  }
}

function genSize(category: string, ar: number, i: number): string {
  if (category === 'Sculpture' || category === 'Installation') {
    return ['h. 120 cm', 'h. 86 cm', 'h. 175 cm', 'h. 54 cm'][i % 4]!;
  }
  const h = [150, 90, 120, 60][i % 4]!;
  const w = Math.round(h * ar);
  return `${w} × ${h} cm`;
}

export type Artist = {
  id: number;
  name: string;
  location: string;
  disciplines: string;
  bio: string;
  accent: string;
  initials: string;
  since: string;
  slug: string;
};

export type Artwork = {
  id: number;
  artistId: number;
  artistName: string;
  artistSlug: string;
  title: string;
  year: number;
  category: string;
  categorySlug: string;
  medium: string;
  tags: string[];
  colors: string[];
  ar: number;
  bg: string;
  size: string;
  slug: string;
};

export function slugify(s: string): string {
  return s
    .toLowerCase()
    .normalize('NFKD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

export const artists: Artist[] = rawArtists.map((a, i) => ({
  id: i,
  name: a[0],
  location: a[1],
  disciplines: a[2],
  bio: a[3],
  accent: palette[a[4]],
  initials: a[0].split(' ').map((s) => s[0]).join(''),
  since: a[5],
  slug: slugify(a[0]),
}));

export const artworks: Artwork[] = rawWorks.map((w, i) => {
  const colors = w[7].split(',').map((k) => palette[k.trim() as PaletteKey]);
  const ar = aspectRatios[i % aspectRatios.length]!;
  return {
    id: i,
    artistId: w[0],
    artistName: rawArtists[w[0]]![0],
    artistSlug: slugify(rawArtists[w[0]]![0]),
    title: w[1],
    year: w[2],
    category: w[3],
    categorySlug: slugify(w[3]),
    medium: w[4],
    tags: w[5].split(','),
    colors,
    ar,
    bg: genBg(w[6], colors),
    size: genSize(w[3], ar, i),
    slug: slugify(w[1]) + '-' + i,
  };
});

export const categories: { name: string; count: number; bg: string; slug: string }[] = (() => {
  const seen = new Map<string, { name: string; count: number; bg: string; slug: string }>();
  for (const w of artworks) {
    const existing = seen.get(w.category);
    if (existing) {
      existing.count += 1;
    } else {
      seen.set(w.category, {
        name: w.category,
        count: 1,
        bg: w.bg,
        slug: slugify(w.category),
      });
    }
  }
  return [...seen.values()];
})();

export function getArtistBySlug(slug: string): Artist | undefined {
  return artists.find((a) => a.slug === slug);
}

export function getArtworkBySlug(slug: string): Artwork | undefined {
  return artworks.find((w) => w.slug === slug);
}

export function getCategoryBySlug(slug: string): { name: string; count: number; slug: string } | undefined {
  return categories.find((c) => c.slug === slug);
}

export function artworksByArtist(artistId: number): Artwork[] {
  return artworks.filter((w) => w.artistId === artistId);
}

export function artworksByCategory(category: string): Artwork[] {
  return artworks.filter((w) => w.category === category);
}

export function artworksByTag(tag: string): Artwork[] {
  return artworks.filter((w) => w.tags.includes(tag));
}

export function artistsByTag(tag: string): Artist[] {
  const ids = new Set(artworksByTag(tag).map((w) => w.artistId));
  return [...ids].map((id) => artists[id]!).filter(Boolean);
}

export const allTags: string[] = [...new Set(artworks.flatMap((w) => w.tags))].sort();

const tagDescriptions: Record<string, string> = {
  Abstract: 'Explore works shaped by color, gesture, geometry and material rather than direct representation.',
  'Color Field': 'Discover expansive areas of color where tone, atmosphere and visual rhythm take the lead.',
  Figurative: 'Browse contemporary works centered on recognizable people, interiors and forms from everyday life.',
  Geometric: 'Find compositions built through line, structure, repetition and carefully balanced shapes.',
  Landscape: 'See artists interpret natural and urban environments through painting, photography, print and drawing.',
  Minimal: 'Explore restrained works where limited materials, forms and gestures create space for close attention.',
  Monochrome: 'Discover works that use a single color family to examine light, texture, contrast and mood.',
  Organic: 'Browse fluid, irregular forms inspired by growth, weather, bodies and the natural world.',
  Portrait: 'Meet contemporary approaches to likeness, identity and presence across painting and drawing.',
  'Still Life': 'Explore arrangements of objects and interiors that turn ordinary subjects into studies of color and time.',
};

export type GalleryTag = {
  name: string;
  slug: string;
  description: string;
  count: number;
  artistCount: number;
};

export const galleryTags: GalleryTag[] = allTags.map((name) => ({
  name,
  slug: slugify(name),
  description: tagDescriptions[name] ?? `Browse contemporary artworks connected by the ${name.toLowerCase()} tag.`,
  count: artworksByTag(name).length,
  artistCount: artistsByTag(name).length,
}));

export function getTagBySlug(slug: string): GalleryTag | undefined {
  return galleryTags.find((tag) => tag.slug === slug);
}
