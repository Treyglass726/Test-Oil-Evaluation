export interface ForecastData {
  date: string;
  temperatureC: number;
  summary: string;
  isDaytime: boolean;
}

export interface GroupedForecast {
  date: string;
  day: ForecastData | null;
  night: ForecastData | null;
}

export type WeatherApiResponse = Array<ForecastData>;
