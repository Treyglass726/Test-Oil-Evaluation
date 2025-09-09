import { WeatherApiResponse } from '../types/weather';

const API_BASE_URL = 'https://localhost:7125'; // Backend URL 

export const weatherApi = {
  async getForecast(address: string): Promise<WeatherApiResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/forecast?address=${encodeURIComponent(address)}`);
      
      if (!response.ok) {
        if (response.status === 400) {
          throw new Error('Please provide a valid address');
        } else if (response.status === 404) {
          throw new Error('Address not found');
        } else {
          throw new Error('Failed to fetch weather data');
        }
      }
      
      return await response.json();
    } catch (error) {
      if (error instanceof TypeError) {
        throw new Error('Unable to connect to weather service. Please check if the backend is running.');
      }
      throw error;
    }
  }
};
