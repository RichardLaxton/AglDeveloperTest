using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace AGL_developer_test
{
    /// <summary>
    /// Simple programming task to respond to the AGL developer test at http://agl-developer-test.azurewebsites.net/
    /// 
    /// Completed by Richard Laxton, 1st September 2016
    /// </summary>
    class Program
    {
        private const string ServiceAddress = @"http://agl-developer-test.azurewebsites.net/people.json";
        static void Main(string[] args)
        {
            Task<IQueryable<Person>> getPersonsTask;
            IQueryable<Person> persons;

            try {
                // Get the async task which will go and get the Person records from the service
                getPersonsTask = GetPersons();

                // Wait for it to run to completion
                getPersonsTask.Wait();

                // Extract the result of the operation
                persons = getPersonsTask.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while retrieving the data from the service.\r\nDetails below:");
                Console.WriteLine(ex);

                return;
            }
            
            // Shape up the data for output
            var report = from p in persons
                         where p.Pets != null
                         group p by p.Gender into g
                         select new {
                             Gender = g.Key,
                             Pets = g.SelectMany(p => p.Pets).OrderBy(p => p.Name)
                         };

            // Output the report to the console
            foreach (var group in report)
            {
                Console.WriteLine(group.Gender);
                foreach (var pet in group.Pets)
                {
                    Console.WriteLine("\t-{0}", pet.Name);
                }
                Console.WriteLine();
            }
        }

        private async static Task<IQueryable<Person>> GetPersons()
        {
            var connection = new HttpClient();
            var response = await connection.GetAsync(ServiceAddress);
            var result = await response.Content.ReadAsAsync<List<Person>>();

            return result.AsQueryable();            
        }
    }

    //
    // Data classes. Would usually be in separate files but included here for simplicity and readability
    //

    public enum Gender
    {
        Female, 
        Male
    }

    public class Person
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }
        public List<Pet> Pets { get; set; }
    }

    public enum PetType
    {
        Cat,
        Dog,
        Fish
    }

    public class Pet
    {
        public string Name { get; set; }
        public PetType Type { get; set; }
    }
}
