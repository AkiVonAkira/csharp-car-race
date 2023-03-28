using System.Diagnostics;

namespace csharp_car_race
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to the car race simulation!");
            Console.WriteLine("Enter the number of cars participating in the race:");
            int numOfCars = int.Parse(Console.ReadLine());

            List<Car> cars = new List<Car>();

            Console.WriteLine("Do you want to use the standard race conditions? (y/n)");
            string standardRace = Console.ReadLine();

            int maxDistance = 0;
            List<string> carNames = new List<string>();

            if (standardRace.ToLower() == "y")
            {
                maxDistance = numOfCars * 100;
                carNames = new List<string>();
                for (int i = 1; i <= numOfCars; i++)
                {
                    carNames.Add($"Car {i}");
                }
            }
            else
            {
                Console.WriteLine("Enter the maximum distance (in km):");
                maxDistance = int.Parse(Console.ReadLine());

                Console.WriteLine($"Enter the {numOfCars} car names:");
                for (int i = 0; i < numOfCars; i++)
                {
                    carNames.Add(Console.ReadLine());
                }
            }

            // Create the car instances
            for (int i = 0; i < numOfCars; i++)
            {
                if (standardRace.ToLower() == "y")
                {
                    // Car name, speed in km/h, distance in meters
                    cars.Add(new Car(carNames[i], 120, 10000));
                }
                else
                {
                    Console.WriteLine($"Enter the speed of {carNames[i]} (in km/h):");
                    double speed = double.Parse(Console.ReadLine());
                    cars.Add(new Car(carNames[i], (int)speed, maxDistance));
                }
            }

            Console.WriteLine("The race is starting!\n");

            Stopwatch sw = Stopwatch.StartNew();
            // Start the race and wait for all cars to finish
            List<Task> tasks = new List<Task>();
            foreach (Car car in cars)
            {
                tasks.Add(car.StartRace());
            }
            await Task.WhenAll(tasks);
            sw.Stop();

            // Determine the winner(s)
            double minTime = double.MaxValue;
            List<string> winners = new List<string>();
            foreach (Car car in cars)
            {
                double timeTaken = car.SimulatedSeconds;
                if (timeTaken < minTime)
                {
                    minTime = timeTaken;
                    winners = new List<string> { car.Name };
                }
                else if (timeTaken == minTime)
                {
                    winners.Add(car.Name);
                }
            }

            // Print the race results
            Console.WriteLine($"Real World Time to complete run: {sw.Elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine("Race Results:");
            foreach (Car car in cars)
            {
                Console.WriteLine($"{car.Name} - {car.Distance / 1000:F2} km, {car.Speed:F2} km/h, Simulated Seconds: {car.SimulatedSeconds} seconds, Real Time: {car.stopwatch.Elapsed.TotalSeconds:F2} seconds. Total Failures: {car.failures}");
            }
            Console.WriteLine();

            if (winners.Count == 1)
            {
                Console.WriteLine($"The winner is {winners[0]}!");
            }
            else
            {
                Console.WriteLine($"It's a tie between {string.Join(", ", winners)}!");
            }
        }
    }
}