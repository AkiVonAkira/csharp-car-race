using System.Diagnostics;

namespace csharp_car_race
{
    class Car
    {
        public string Name { get; private set; }
        public int Distance { get; private set; }
        public int MaxDistance { get; private set; }
        public int SimulatedSeconds { get; private set; }
        public double Speed { get; private set; } // speed in m/s
        public Stopwatch stopwatch { get; private set; }
        private Random random = new Random();
        public bool finished { get; private set; }
        private bool problemOccured;
        private int problemTime;
        private string problemType;
        private int subTicks;
        private int subTickTime;
        public int failures { get; private set; }

        public Car(string name, int speed, int maxDistance)
        {
            Name = name;
            Distance = 0;
            MaxDistance = maxDistance;
            Speed = speed;
            SimulatedSeconds = 0;
            random = new Random();
            finished = false;
            problemOccured = false;
            problemTime = 0;
            problemType = "";
            subTicks = 0;
            subTickTime = 100;
            failures = 0;
        }

        public async Task StartRace()
        {
            //Console.WriteLine($"{Name} starts the race!");
            //await Console.Out.WriteLineAsync($"Speed: {Speed}, maxDistance {MaxDistance}, Distance {Distance}, ");
            stopwatch = Stopwatch.StartNew();
            while (!finished)
            {
                subTicks++;
                if (subTicks == subTickTime)
                {
                    if (problemOccured && subTicks == problemTime)
                    {
                        ResetProblemAsync();
                    }

                    await MoveAsync(10);
                }
                else
                {
                    await MoveAsync(10);
                }


                // Print status every 5 seconds
                if (stopwatch.ElapsedMilliseconds >= 5000)
                {
                    double distanceInKm = (double)Distance / 1000;
                    //Console.WriteLine($"{Name} has traveled {distanceInKm:F2} km so far. Its current speed is: {Speed:F2} km/h and it has been driving for {SimulatedSeconds} seconds");
                    stopwatch.Restart();
                }
            }
            stopwatch.Stop();
            //Console.WriteLine($"{Name} finished the race in {SimulatedSeconds} seconds and {stopwatch.Elapsed.TotalSeconds:F2} real life seconds!");
        }

        private async Task MoveAsync(int delayTime)
        {
            await Task.Delay(delayTime);

            SimulatedSeconds++;

            Distance += (int)(Speed / 3.6); // convert speed from km/h to m/s

            if (Distance >= MaxDistance && !finished)
            {
                finished = true;
            }

            subTicks++;

            if (subTicks == 30)
            {
                int eventNumber = random.Next(50);

                if (eventNumber == 0)
                {
                    await HandleProblemAsync("ran out of gas", 30);
                }
                else if (eventNumber <= 2)
                {
                    await HandleProblemAsync("got a flat tire", 20);
                }
                else if (eventNumber <= 7)
                {
                    await HandleProblemAsync("got a bird on the windshields", 10);
                }
                else if (eventNumber <= 17)
                {
                    //await HandleProblemAsync("has a mechanical problem", 0);
                    //Console.WriteLine($"{Name} has a mechanical problem! It will move 1km/h slower.");
                    failures++;
                    Speed -= 1;
                }

                subTicks = 0;
            }
            //else if (subTicks == 15)
            //{
            //    int eventNumber = random.Next(50);

            //    if (eventNumber == 0)
            //    {
            //        await HandleProblemAsync("ran out of gas", 30);
            //    }
            //    else if (eventNumber <= 2)
            //    {
            //        await HandleProblemAsync("got a flat tire", 20);
            //    }
            //    else if (eventNumber <= 7)
            //    {
            //        await HandleProblemAsync("got a bird on the windshields", 10);
            //    }

            //    subTicks = 0;
            //}
        }

        private async Task HandleProblemAsync(string type, int time)
        {
            failures++;
            problemType = type;
            problemTime = time;
            problemOccured = true;
            //Console.WriteLine($"{Name} {type}! It will take {time} seconds to fix.");

            await Task.Delay(time * 10);
            SimulatedSeconds += time;

            //Console.WriteLine($"{Name}'s problem has been fixed! It can now resume the race.");
            problemOccured = false;
            problemType = "";
            problemTime = 0;
        }

        private void ResetProblemAsync()
        {
            //Console.WriteLine($"{Name} has fixed the {problemType} and is back on track!");
            problemOccured = false;
            //problemType = "";
            Speed += problemType switch
            {
                "ran out of gas" => 20,
                "got a flat tire" => 10,
                "got a bird on the windshield" => 5,
                "has a mechanical problem" => 1,
                _ => 0
            };
        }
    }

}
