using System.Diagnostics;

namespace csharp_car_race
{
    class Program
    {
        public static List<Car> cars { get; set; }
        public static Stopwatch Stopwatch = new Stopwatch();
        public static int _mode;

        static async Task Main(string[] args)
        {
            int _mode = SelectMode();

            Console.WriteLine("Enter the number of cars participating in the race:");
            int numOfCars = int.Parse(Console.ReadLine());

            cars = new List<Car>();

            Console.WriteLine("Do you want to use the standard race conditions? (y/n) [10km Race, 120km/h]");
            string standardRace = Console.ReadLine();

            int maxDistance = 10000;
            int carSpeed = 120;
            List<string> carNames = new List<string>();

            if (standardRace.ToLower() == "y")
            {
                carNames = new List<string>();
                for (int i = 1; i <= numOfCars; i++)
                {
                    carNames.Add($"Car {i}");
                }
            }
            else if (standardRace.ToLower() == "n" && _mode == 1)
            {
                Console.WriteLine("Enter the maximum distance (in km):");
                maxDistance = int.Parse(Console.ReadLine()) * 1000;
                Console.WriteLine("Enter the car speed (in km/h):");
                carSpeed = int.Parse(Console.ReadLine());
                carNames = new List<string>();
                for (int i = 1; i <= numOfCars; i++)
                {
                    carNames.Add($"Car {i}");
                }
            }
            else if (standardRace.ToLower() == "n" && _mode == 2)
            {
                Console.WriteLine("Enter the maximum distance (in km):");
                maxDistance = int.Parse(Console.ReadLine()) * 1000;
                Console.WriteLine("Enter the car speed (in km/h):");
                carSpeed = int.Parse(Console.ReadLine());

                Console.WriteLine($"Enter the {numOfCars} car names:");
                for (int i = 0; i < numOfCars; i++)
                {
                    Console.WriteLine($"Enter car name for Car nr. {i + 1}");
                    carNames.Add(Console.ReadLine());
                }
            }

            // Create the car instances
            for (int i = 0; i < numOfCars; i++)
            {
                if (standardRace.ToLower() == "y")
                {
                    // Car name, speed in km/h, distance in meters
                    cars.Add(new Car(carNames[i], carSpeed, maxDistance));
                }
                else
                {
                    cars.Add(new Car(carNames[i], carSpeed, maxDistance));
                }
            }

            // Create and start the progress menu task
            var cancellationTokenSource = new CancellationTokenSource();
            var progressMenuTask = UpdateProgressMenu(cancellationTokenSource.Token);

            Stopwatch.Start();
            if (_mode == 1)
            {
                await RunSimulation();
            }
            else if (_mode == 2)
            {
                await RunRealTime();
            }

            Stopwatch.Stop();
            Task.Delay(2000).Wait();
            cancellationTokenSource.Cancel();
            await progressMenuTask;

            Console.ReadLine();
        }

        private static int SelectMode()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Car Race Simulation!\n");
            Console.WriteLine("Choose a Mode: \n(1) Simulation \n(2) Real Time");
            int mode = int.Parse(Console.ReadLine());
            if (mode != 1 && mode != 2)
            {
                Console.WriteLine("Invalid mode selection. Try again in 1 second.");
                Thread.Sleep(1000);
                SelectMode();
            }
            return mode;
        }

        static async Task RunSimulation()
        {
            List<Task> tasks = new List<Task>();

            foreach (var car in cars)
            {
                tasks.Add(car.StartRace(10));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        static async Task RunRealTime()
        {
            List<Task> tasks = new List<Task>();

            foreach (var car in cars)
            {
                tasks.Add(car.StartRace(1000));
            }
            await Task.WhenAll(tasks.ToArray());
        }

        private static async Task UpdateProgressMenu(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Clear();

                await PrintProgressBar();

                // Print the race results
                Console.WriteLine($"Elapsed Time: {Program.Stopwatch.Elapsed.TotalSeconds:F2} seconds\n");

                //var sortedCars = await GetRaceWinners(); 
                var sortedCars = cars.OrderBy(car => car.actualRaceTime).ThenByDescending(car => car.Distance);


                string winners = await GetRaceWinners();
                Console.WriteLine(winners);
                string leastFailures = await DetermineLeastFailures();
                Console.WriteLine(leastFailures);

                Console.WriteLine();

                // Display information for the top cars based on lowest time
                int consoleHeight = Console.WindowHeight;
                int rowsPerCar = 4; // Each car takes 5 rows (name, distance, speed/time)

                int maxCarsToShow = consoleHeight / rowsPerCar - 3; // Leave room for other output

                Console.WriteLine($"{"Car Name",-20} {"Distance (km)",-20} {"Speed (km/h)",-20} {"Time (s)",-20}");
                Console.WriteLine(new string('-', Console.WindowWidth));

                foreach (var car in sortedCars.Take(maxCarsToShow))
                {
                    double distanceInKm = (double)car.Distance / 1000;
                    Console.WriteLine($"{car.Name,-20} {distanceInKm,-20:F2} {car.Speed,-20:F2} {car.SimulatedSeconds,-20}");

                    if (car.problemOccured)
                    {
                        Console.WriteLine($"{car.Name} has a {car.problemType}! It will take {car.problemTime} seconds to fix.");
                    }

                    if (car.failures > 0)
                    {
                        Console.WriteLine($"{car.Name} has experienced {car.failures} failures so far.");
                    }

                    Console.WriteLine(new string('-', Console.WindowWidth));
                }

                await Task.Delay(1000); // Wait for 1000 milisecond before updating the progress menu again
            }
        }

        private static async Task PrintProgressBar()
        {
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
            int progressBarWidth = 50;
            int completedWidth = (int)Math.Round((double)progressBarWidth / 100 * progressPercentage);
            int remainingWidth = progressBarWidth - completedWidth;

            Console.Write("[");
            Console.Write(new string('#', completedWidth));
            Console.Write(new string(' ', remainingWidth));
            Console.Write("]\n");
            await Task.Delay(10);
        }

        static async Task<string> GetRaceWinners()
        {
            var winners = cars.Where(c => c.finished)
                              .OrderBy(c => c.actualRaceTime)
                              .ThenByDescending(c => c.Distance)
                              .Take(10)
                              .ToList();

            if (winners.Count == 1)
            {
                return $"{winners[0].Name} won the race with a time of {winners[0].actualRaceTime:F2}s and a distance of {winners[0].Distance:F2}m.";
            }
            else if (winners.Count > 1 && winners.Count <= 10)
            {
                var winnerNames = string.Join(", ", winners.Select(w => w.Name));
                return $"The winners are {winnerNames}.";
            }
            else if (winners.Count > 10)
            {
                return $"There are too many winners right now.";
            }
            else
            {
                return $"There are no winners yet.";
            }
        }

        static async Task<string> DetermineLeastFailures()
        {
            int minFailures = int.MaxValue;
            List<Car> leastFailuresCars = new List<Car>();
            List<Car> carsWithFailures = new List<Car>();

            foreach (Car car in cars)
            {
                int failures = car.failures;
                if (failures < minFailures)
                {
                    minFailures = failures;
                    leastFailuresCars = new List<Car> { car };
                }
                else if (failures == minFailures)
                {
                    leastFailuresCars.Add(car);
                }

                if (failures > 0)
                {
                    carsWithFailures.Add(car);
                }
            }


            if (carsWithFailures.Count == 0)
            {
                return "No cars have failures.";
            }
            else if (carsWithFailures.Count == 1)
            {
                Car car = carsWithFailures[0];
                return $"{car.Name} has the least failures with {car.failures} failure(s).";
            }
            else if (carsWithFailures.Count <= 10)
            {
                string carNames = string.Join(", ", carsWithFailures.Select(c => c.Name));
                return $"Cars with failures: {carNames}.";
            }
            else
            {
                int maxFailures = carsWithFailures.Max(car => car.failures);
                return $"{carsWithFailures.Count} Cars with failures. Lowest count is {minFailures}. Highest count is {maxFailures}.";
            }
        }
    }
}