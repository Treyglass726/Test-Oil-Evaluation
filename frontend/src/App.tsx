import React, { useState, FormEvent } from 'react';
import {
  Container,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  CircularProgress,
  ThemeProvider,
  createTheme,
  CssBaseline
} from '@mui/material';
// Removed @mui/icons-material dependency
import WeatherCard from './components/WeatherCard';
import { weatherApi } from './services/weatherApi';
import { ForecastData, GroupedForecast } from './types/weather';
import './App.css';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

const App: React.FC = () => {
  const [address, setAddress] = useState<string>('');
  const [forecast, setForecast] = useState<GroupedForecast[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    if (!address.trim()) {
      setError('Please enter an address');
      return;
    }

    setLoading(true);
    setError('');
    setForecast([]);

    try {
      const data = await weatherApi.getForecast(address);
      // Group forecasts by date
      const groupedForecasts = groupForecastsByDate(data);
      setForecast(groupedForecasts);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setLoading(false);
    }
  };

  const groupForecastsByDate = (forecasts: ForecastData[]): GroupedForecast[] => {
    const grouped: { [key: string]: GroupedForecast } = {};
    
    forecasts.forEach(forecast => {
      const dateKey = forecast.date;
      if (!grouped[dateKey]) {
        grouped[dateKey] = {
          date: dateKey,
          day: null,
          night: null
        };
      }
      
      if (forecast.isDaytime) {
        grouped[dateKey].day = forecast;
      } else {
        grouped[dateKey].night = forecast;
      }
    });
    
    return Object.values(grouped);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Typography variant="h3" component="h1" gutterBottom align="center">
          Weather Forecast
        </Typography>
        
        <Box component="form" onSubmit={handleSubmit} sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            <TextField
              fullWidth
              label="Enter Address"
              variant="outlined"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="e.g., 123 Main St, New York, NY"
              disabled={loading}
            />
            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} /> : null}
              sx={{ minWidth: 120 }}
            >
              {loading ? 'Loading...' : 'Search'}
            </Button>
          </Box>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {forecast.length > 0 && (
          <Box>
            <Typography variant="h4" component="h2" gutterBottom>
              7-Day Forecast
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center' }}>
              {forecast.map((dayGroup, index) => (
                <Box 
                  key={index}
                  sx={{
                    flexBasis: {
                      xs: '100%',
                      sm: '50%',
                      md: '33.33%',
                      lg: '25%'
                    },
                    padding: 1,
                    mb: 3
                  }}
                >
                  <WeatherCard 
                    dayForecast={dayGroup.day}
                    nightForecast={dayGroup.night}
                    date={dayGroup.date}
                  />
                </Box>
              ))}
            </Box>
          </Box>
        )}
      </Container>
    </ThemeProvider>
  );
};

export default App;
