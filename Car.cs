namespace csharp_car_race
{
    class Car
    {
        public string Name { get; private set; }
        public int Distance { get; private set; }
        public int MaxDistance { get; private set; }
        public int SimulatedSeconds { get; private set; }
        public double Speed { get; private set; } // speed in m/s
        public double actualRaceTime { get; private set; }
        private Random random = new Random();
        public bool finished { get; private set; }
        public bool problemOccured { get; private set; }
        public int problemTime { get; private set; }
        public string problemType { get; private set; }
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
            actualRaceTime = 0;
            random = new Random();
            finished = false;
            problemOccured = false;
            problemTime = 0;
            problemType = "";
            subTicks = 0;
            subTickTime = 1;
            failures = 0;
        }

        public async Task StartRace(int delayTime)
        {
            while (!finished)
            {
                subTicks++;
                if (subTicks == subTickTime)
                {
                    if (problemOccured && subTicks == problemTime)
                    {
                        ResetProblemAsync();
                    }

                    await MoveAsync(delayTime);
                }
                else
                {
                    await MoveAsync(delayTime);
                }

                if (finished)
                {
                    actualRaceTime = (double)SimulatedSeconds / 60;
                }
            }
        }

        private async Task MoveAsync(int delayTime)
        {
            await Task.Delay(delayTime);

            SimulatedSeconds++;
            subTicks++;

            Distance += (int)(Speed / 3.6); // convert speed from km/h to m/s

            if (Distance >= MaxDistance && !finished)
            {
                finished = true;
            }

            if (subTicks == 30)
            {
                int eventNumber = random.Next(50);

                if (eventNumber == 0)
                {
                    await HandleProblemAsync("ran out of gas", 30, delayTime);
                    actualRaceTime += 0.5; // add penalty time for running out of gas
                }
                else if (eventNumber <= 2)
                {
                    await HandleProblemAsync("got a flat tire", 20, delayTime);
                    actualRaceTime += 0.3; // add penalty time for getting a flat tire
                }
                else if (eventNumber <= 7)
                {
                    await HandleProblemAsync("got a bird on the windshields", 10, delayTime);
                    actualRaceTime += 0.1; // add penalty time for getting a bird on the windshield
                }
                else if (eventNumber <= 17)
                {
                    await HandleProblemAsync("has a mechanical problem", 0, delayTime);
                    Speed -= 1;
                }

                subTicks = 0;
            }
        }

        private async Task HandleProblemAsync(string type, int time, int delayTime)
        {
            failures++;
            problemType = type;
            problemTime = time;
            problemOccured = true;

            await Task.Delay(time * delayTime);
            SimulatedSeconds += time;

            problemOccured = false;
            problemType = "";
            problemTime = 0;
        }

        private void ResetProblemAsync()
        {
            problemOccured = false;
            problemType = "";
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
