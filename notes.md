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






Notes while building:
.net 9.0 
https://learning.oreilly.com/library/view/c-13-and/9781835881224/Text/Chapter_04.xhtml#_idParaDest-250

TTD follows same principles as c++ 


API architecture 
https://learning.oreilly.com/library/view/mastering-api-architecture/9781492090625/

chapter 2: testing 

types of testing with api's

Q1: 
unit and components testing 
verify that service has been created and works 

unit tests - small isolated units of code 

service tests - verify that service has been created and works 

end to end test with UI

API Components testing:
possilbe test case to look for:
is the correct status code returned
does the repsone contain the correct data
is tan incoming payload rejected if null or empty parm passed in 
when i send a request where the accepted contet type is XML, will the data return the expected format 
if a request is made by a yser who does not hvae the correct entitlement what will the response be?
what will happen if an empty dataset is returned 
when creating a resource does the location header point to a new asset created

