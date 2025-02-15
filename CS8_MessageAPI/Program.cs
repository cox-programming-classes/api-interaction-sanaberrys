﻿global using ErrorAction = System.Action<CS8_MessageAPI.Models.ErrorRecord>;
using CS8_MessageAPI.Models;
using CS8_MessageAPI.Services;

// This is your PostMan Analog
var apiService = new ApiService();

var loginSuccess = true;

await apiService.Login("sana.khan@winsor.edu", "002@*$jmoHMT",
    err =>
    {
        Console.WriteLine(err);
        loginSuccess = false;
    });
    
if(!loginSuccess)
    return;

var assesments = await apiService.SendAsync<AssesmentCalendar[]>(HttpMethod.Get,"api/assessment-calendar",err =>
{
    Console.WriteLine(err);
    loginSuccess = false;
});;

foreach (var assesment in assesments)
{
Console.WriteLine(assesment);

}