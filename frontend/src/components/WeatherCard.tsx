import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box
} from '@mui/material';
import { ForecastData } from '../types/weather';

interface WeatherCardProps {
  dayForecast: ForecastData | null;
  nightForecast: ForecastData | null;
  date: string;
}

interface ForecastSubCardProps {
  forecast: ForecastData;
  label: string;
  bgColor: string;
}

const WeatherCard: React.FC<WeatherCardProps> = ({ dayForecast, nightForecast, date }) => {
  const getWeatherIcon = (summary: string, isDaytime: boolean): React.ReactNode => {
    const summaryLower = summary.toLowerCase();
    const iconStyle = { fontSize: '32px', fontWeight: 'bold' };
    
    if (summaryLower.includes('rain') || summaryLower.includes('shower')) {
      return <span style={{ ...iconStyle, color: '#1976d2' }}>🌧️</span>;
    } else if (summaryLower.includes('snow')) {
      return <span style={{ ...iconStyle, color: '#90caf9' }}>❄️</span>;
    } else if (summaryLower.includes('thunder') || summaryLower.includes('storm')) {
      return <span style={{ ...iconStyle, color: '#424242' }}>⚡</span>;
    } else if (summaryLower.includes('cloud') || summaryLower.includes('overcast')) {
      return <span style={{ ...iconStyle, color: '#757575' }}>☁️</span>;
    } else if (isDaytime) {
      return <span style={{ ...iconStyle, color: '#ff9800' }}>☀️</span>;
    } else {
      return <span style={{ ...iconStyle, color: '#3f51b5' }}>🌙</span>;
    }
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      weekday: 'long', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  const convertToFahrenheit = (celsius: number): number => {
    return Math.round((celsius * 9/5) + 32);
  };

  const ForecastSubCard: React.FC<ForecastSubCardProps> = ({ forecast, label, bgColor }) => (
    <Card variant="outlined" sx={{ 
      backgroundColor: bgColor, 
      height: 180, 
      width: '100%',
      minWidth: 120,
      maxWidth: 120
    }}>
      <CardContent sx={{ 
        p: 1.5, 
        height: '100%', 
        display: 'flex', 
        flexDirection: 'column',
        justifyContent: 'space-between',
        width: '100%'
      }}>
        <Typography variant="subtitle1" component="div" gutterBottom align="center" fontWeight="bold">
          {label}
        </Typography>
        
        <Box sx={{ 
          display: 'flex', 
          flexDirection: 'column', 
          alignItems: 'center', 
          gap: 1,
          flexGrow: 1,
          justifyContent: 'center'
        }}>
          {getWeatherIcon(forecast.summary, forecast.isDaytime)}
          
          <Typography variant="h5" component="div">
            {convertToFahrenheit(forecast.temperatureC)}°F
          </Typography>
        </Box>
        
        <Box sx={{ 
          height: 40, 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          width: '100%',
          px: 1
        }}>
          <Typography 
            variant="body2" 
            color="text.secondary" 
            align="center"
            sx={{
              fontSize: '0.75rem',
              lineHeight: 1.2,
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              width: '100%',
              maxWidth: '100%'
            }}
          >
            {forecast.summary}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );

  return (
    <Card sx={{ 
      width: '100%', 
      height: '100%',
      elevation: 3,
      display: 'flex',
      flexDirection: 'column'
    }}>
      <CardContent sx={{ flexGrow: 1, p: 2 }}>
        <Typography variant="h6" component="div" gutterBottom align="center" fontWeight="bold">
          {formatDate(date)}
        </Typography>
        
        <Box sx={{ 
          display: 'flex', 
          gap: 2, 
          justifyContent: 'center',
          alignItems: 'stretch',
          height: '100%'
        }}>
          {dayForecast && (
            <ForecastSubCard 
              forecast={dayForecast} 
              label="Day" 
              bgColor="#fff3e0"
            />
          )}
          
          {nightForecast && (
            <ForecastSubCard 
              forecast={nightForecast} 
              label="Night" 
              bgColor="#e8eaf6"
            />
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default WeatherCard;
