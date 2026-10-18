
namespace workoutTracker.models
{
    public class Workout
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<Exercise> Exercises { get; set; } = new();

        public Workout()
        {
        }

        public Workout(string name)
        {
            Name = name;
        }

        public void AddExercise(Exercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            if (!Exercises.Contains(exercise))
            {
                Exercises.Add(exercise);
            }
        }
    }
}
