using System.Diagnostics;

namespace csharp_car_race
{
    class Program
    {

        public static Stopwatch Stopwatch = new Stopwatch();

        static async Task Main(string[] args)
        {
            Console.Clear();
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

            // Create and start the progress menu task
            UpdateProgressMenu(cars);

            Console.WriteLine("The race is starting!\n");

            Stopwatch.Start();
            // Start the race and wait for all cars to finish
            List<Task> tasks = new List<Task>();
            foreach (Car car in cars)
            {
                tasks.Add(car.StartRace());
            }
            await Task.WhenAll(tasks);
            Stopwatch.Stop();
            Console.ReadLine();

        }

        private static async void UpdateProgressMenu(List<Car> cars)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Simulation Progress");
                Console.WriteLine("-------------------");

                // Calculate the progress percentage
                int completedCars = 0;
                foreach (Car car in cars)
                {
                    if (car.finished)
                    {
                        completedCars++;
                    }
                }
                int progressPercentage = (int)Math.Round((double)completedCars / cars.Count * 100);

                Console.WriteLine($"Completed: {completedCars}/{cars.Count} ({progressPercentage}%)");

                // Print the progress bar
                int progressBarWidth = 30;
                int completedWidth = (int)Math.Round((double)progressBarWidth / 100 * progressPercentage);
                int remainingWidth = progressBarWidth - completedWidth;

                Console.Write("[");
                Console.Write(new string('#', completedWidth));
                Console.Write(new string(' ', remainingWidth));
                Console.Write("]\n");

                // Determine the winner(s)
                double minTime = double.MaxValue;
                List<int> winners = new List<int>();
                foreach (Car car in cars)
                {
                    double timeTaken = car.SimulatedSeconds;
                    if (timeTaken < minTime)
                    {
                        minTime = timeTaken;
                        winners = new List<int> { cars.IndexOf(car) };
                    }
                    else if (timeTaken == minTime)
                    {
                        winners.Add(cars.IndexOf(car));
                    }
                }

                // Determine the car(s) with the least failures
                int minFailures = int.MaxValue;
                List<string> leastFailuresCars = new List<string>();
                foreach (Car car in cars)
                {
                    int failures = car.failures;
                    if (failures < minFailures)
                    {
                        minFailures = failures;
                        leastFailuresCars = new List<string> { car.Name };
                    }
                    else if (failures == minFailures)
                    {
                        leastFailuresCars.Add(car.Name);
                    }
                }


                // Print the race results
                Console.WriteLine($"Elapsed Time: {Program.Stopwatch.Elapsed.TotalSeconds:F2} seconds\n");
                Console.WriteLine("Simulation Results:\n");

                //foreach (Car car in cars)
                //{
                //    Console.WriteLine($"{car.Name} - {car.Distance / 1000:F2} km, {car.Speed:F2} km/h, Simulated Seconds: {car.SimulatedSeconds:F2} seconds, Real Time: {car.stopwatch.Elapsed.TotalSeconds:F2} seconds, Failures: {car.failures}");
                //}

                if (winners.Count == 1)
                {
                    Console.WriteLine($"The fastest car is {winners[0]}! With a completion time of {cars[winners[0]].SimulatedSeconds:F0} seconds.");
                }
                else if (winners.Count <= 20)
                {
                    Console.WriteLine($"Fastest Car(s) {string.Join(", ", winners)}! With a completion time of {cars[winners[0]].SimulatedSeconds:F0} seconds.");
                }
                else
                {
                    Console.WriteLine($"{winners.Count} Winners right now");
                }

                if (leastFailuresCars.Count == 1)
                {
                    Console.WriteLine($"The car with the least failures is {leastFailuresCars[0]}!");
                }
                else if (leastFailuresCars.Count <= 20)
                {
                    Console.WriteLine($"Car(s) with the least failures {string.Join(", ", leastFailuresCars)}!");
                }
                else
                {
                    Console.WriteLine($"{winners.Count} Cars with failures right now");
                }


                await Task.Delay(1000); // Wait for 1000 milisecond before updating the progress menu again
            }
        }
    }
}