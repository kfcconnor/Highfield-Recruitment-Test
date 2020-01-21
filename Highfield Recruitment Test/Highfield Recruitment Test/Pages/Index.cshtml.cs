using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Highfield_Recruitment_Test
{
    public class tableDisplayModel : PageModel
    {
        public static List<userEntity> userData;
        public static List<AgePlusTwenty> ages;
        public static List<TopColours> tC;
        public static string responseCode = " ";
        public async static Task OnGetAsync()
        {
            
            dataProcessingFunctions data = new dataProcessingFunctions();
            userData = await apiCalls.updateUserData();
            //calls API handler to get user data from the API and convert it from JSON to a class
            (ages, tC) = data.processTable(userData);
            //uses the data from the API Call to carry out the calculations on the user set.

        }

        public async Task<IActionResult> OnPost()
        {
            Response responseToSend = new Response(userData, ages, tC);
            responseCode = "Response Code: " + await apiCalls.postResponse(responseToSend);
            //sends the response to API Handler to be turned back into json and sent to the server.
            return Page();
        }

    }
}