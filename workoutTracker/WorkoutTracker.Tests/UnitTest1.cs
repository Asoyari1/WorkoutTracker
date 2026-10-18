using workoutTracker.models;

namespace WorkoutTracker.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ExerciseConstructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var exercise = new Exercise("Bench Press", "Chest");

            // Assert
            Assert.Equal("Bench Press", exercise.Name);
            Assert.Equal("Chest", exercise.MuscleGroup);
        }

        [Fact]
        public void AddExercise_AddsExerciseToWorkout()
        {
            // Arrange
            var workout = new Workout("Push Day");
            var exercise = new Exercise("Bench Press", "Chest");

            // Act
            workout.AddExercise(exercise);

            // Assert
            Assert.Equal(1, workout.Exercises.Count);
            Assert.Contains(exercise, workout.Exercises);
        }

        [Fact]
        public void AddExercise_DoesNotAddSameObjectTwice()
        {
            // Arrange
            var workout = new Workout("Push Day");
            var exercise = new Exercise("Bench Press", "Chest");

            // Act
            workout.AddExercise(exercise);
            workout.AddExercise(exercise);

            // Assert
            Assert.Equal(1, workout.Exercises.Count);
        }
        [Fact]
        public void AddExercise_ThrowsException_WhenExerciseIsNull()
        {
            // Arrange
            var workout = new Workout("Push Day");

            // Act and Assert
            Assert.Throws<ArgumentNullException>(
                () => workout.AddExercise(null!));
        }
    }
}