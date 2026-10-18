export interface IotTile {
  id: string;
  group: string;
  row: number;
  column: number;
  name: string;
}

const tile = (group: string, row: number, column: number, name: string): IotTile => ({
  id: `${group}-${String(row).padStart(2, '0')}-${String(column).padStart(2, '0')}`,
  group,
  row,
  column,
  name,
});

export const IOT_TILES: IotTile[] = [
  tile('A', 10, 1, 'Invernadero Norte · 10-1'),
  tile('A', 10, 2, 'Invernadero Norte · 10-2'),
  tile('A', 10, 3, 'Invernadero Norte · 10-3'),
  tile('A', 10, 4, 'Invernadero Norte · 10-4'),
  tile('A', 11, 1, 'Invernadero Norte · 11-1'),
  tile('A', 11, 2, 'Invernadero Norte · 11-2'),
  tile('A', 11, 3, 'Invernadero Norte · 11-3'),
  tile('A', 11, 4, 'Invernadero Norte · 11-4'),
  tile('A', 12, 1, 'Invernadero Norte · 12-1'),
  tile('A', 12, 2, 'Invernadero Norte · 12-2'),
  tile('A', 12, 3, 'Invernadero Norte · 12-3'),
  tile('A', 12, 4, 'Invernadero Norte · 12-4'),
  tile('B', 20, 1, 'Jardín Central · 20-1'),
  tile('B', 20, 2, 'Jardín Central · 20-2'),
  tile('B', 20, 3, 'Jardín Central · 20-3'),
  tile('B', 20, 4, 'Jardín Central · 20-4'),
  tile('B', 21, 1, 'Jardín Central · 21-1'),
  tile('B', 21, 2, 'Jardín Central · 21-2'),
  tile('B', 21, 3, 'Jardín Central · 21-3'),
  tile('B', 21, 4, 'Jardín Central · 21-4'),
];

export interface IotTileGroup {
  group: string;
  tiles: IotTile[];
  maxColumn: number;
  minRow: number;
}

export const groupTiles = (tiles: IotTile[]): IotTileGroup[] => {
  const map = new Map<string, IotTile[]>();

  for (const item of tiles) {
    const bucket = map.get(item.group);
    if (bucket) {
      bucket.push(item);
    } else {
      map.set(item.group, [item]);
    }
  }

  return Array.from(map.entries())
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([group, items]) => {
      const sorted = [...items].sort(
        (left, right) => left.row - right.row || left.column - right.column,
      );

      return {
        group,
        tiles: sorted,
        maxColumn: Math.max(...sorted.map((item) => item.column)),
        minRow: Math.min(...sorted.map((item) => item.row)),
      };
    });
};

export const isKnownTile = (area: string): boolean =>
  IOT_TILES.some((item) => item.id === area);
