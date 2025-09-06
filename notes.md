Think about the technical design of the solution
what do we need from the apis 

functional Requirements for backend api:
accept an address as a parameter
validate the address
return an error if the address is invalid
geocode the address using census api
get the zip code from the geocode response
get the forecast from the weather api
return the forecast as 7 daily items, have the front end just display the items( make one component and reuse it)

Non-functional Requirements for backend api:

rate limit the number of requests to the apis
unit testing 

weather 
think about rate limits -- does the api return an error if we exceed the limit? do we need to implement a queue to handle requests?


https://api.weather.gov/points/{latitude},{longitude}
api endpoints
forecast - forecast for 12h periods over the next seven days
forecastHourly - forecast for hourly periods over the next seven days
forecastGridData - raw forecast data over the next seven days

My api should just have one endpoint
/api/forecast

should take in address
get the geocode address from the census api
get the forecast from the weather api
return the forecast as 7 dailiy items, have the front end just display the items( make one componente and reuse it)

make a high level with vi


and how the different components fit together
and who your audience is.