import type moment from "moment";
import type { Entity } from "../models/entity";

export const getColor = (index: number): string => {
  const hue = (index * 137.508) % 360;
  const sat = 70;
  const light = index % 2 === 0 ? 45 : 65;
  return `hsl(${hue}, ${sat}%, ${light}%)`;
};

export const getGraphTitle = (
  entities: Entity[],
  from: moment.Moment,
  to?: moment.Moment
): string => {
  const datesToShow = to
    ? `${from.format("DD.MM.YYYY")} -> ${to.format("DD.MM.YYYY")}`
    : `${from.format("DD.MM.YYYY")} ->`;
  const entitiesToShow = entities.map((l) => l.displayName);
  return `${datesToShow} (${entitiesToShow.join(", ")})`;
};
