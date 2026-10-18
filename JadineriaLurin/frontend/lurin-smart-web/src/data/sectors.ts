import type { Sector } from '../types/irrigation';

const sector = (
  group: string,
  row: number,
  column: number,
  deviceId: string,
  name: string,
): Sector => ({
  id: `${group}-${String(row).padStart(2, '0')}-${String(column).padStart(2, '0')}`,
  group,
  row,
  column,
  deviceId,
  name,
});

export const SECTORS: Sector[] = [
  sector('A', 10, 1, 'esp32-01', 'Invernadero Norte · 10-1'),
  sector('A', 10, 2, 'esp32-01', 'Invernadero Norte · 10-2'),
  sector('A', 10, 3, 'esp32-01', 'Invernadero Norte · 10-3'),
  sector('A', 10, 4, 'esp32-01', 'Invernadero Norte · 10-4'),
  sector('A', 11, 1, 'esp32-01', 'Invernadero Norte · 11-1'),
  sector('A', 11, 2, 'esp32-01', 'Invernadero Norte · 11-2'),
  sector('A', 11, 3, 'esp32-01', 'Invernadero Norte · 11-3'),
  sector('A', 11, 4, 'esp32-01', 'Invernadero Norte · 11-4'),
  sector('A', 12, 1, 'esp32-01', 'Invernadero Norte · 12-1'),
  sector('A', 12, 2, 'esp32-01', 'Invernadero Norte · 12-2'),
  sector('A', 12, 3, 'esp32-01', 'Invernadero Norte · 12-3'),
  sector('A', 12, 4, 'esp32-01', 'Invernadero Norte · 12-4'),
  sector('B', 20, 1, 'esp32-02', 'Jardín Central · 20-1'),
  sector('B', 20, 2, 'esp32-02', 'Jardín Central · 20-2'),
  sector('B', 20, 3, 'esp32-02', 'Jardín Central · 20-3'),
  sector('B', 20, 4, 'esp32-02', 'Jardín Central · 20-4'),
  sector('B', 21, 1, 'esp32-02', 'Jardín Central · 21-1'),
  sector('B', 21, 2, 'esp32-02', 'Jardín Central · 21-2'),
  sector('B', 21, 3, 'esp32-02', 'Jardín Central · 21-3'),
  sector('B', 21, 4, 'esp32-02', 'Jardín Central · 21-4'),
];

export interface SectorGroup {
  group: string;
  deviceId: string;
  sectors: Sector[];
  maxColumn: number;
  minRow: number;
}

export const groupSectors = (sectors: Sector[]): SectorGroup[] => {
  const map = new Map<string, Sector[]>();

  for (const item of sectors) {
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
        deviceId: sorted[0].deviceId,
        sectors: sorted,
        maxColumn: Math.max(...sorted.map((item) => item.column)),
        minRow: Math.min(...sorted.map((item) => item.row)),
      };
    });
};

export const uniqueDeviceIds = (sectors: Sector[]): string[] => [
  ...new Set(sectors.map((item) => item.deviceId)),
];
