using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using FitnessTracker.Domain;

namespace FitnessTracker.Infrastructure
{
    [DataContract]
    public class WorkoutDto
    {
        [DataMember] public Guid Id { get; set; }
        [DataMember] public Guid UserId { get; set; }
        [DataMember] public string Title { get; set; }
        [DataMember] public int Type { get; set; }
        [DataMember] public DateTime StartedAt { get; set; }
        [DataMember] public DateTime? CompletedAt { get; set; }
        [DataMember] public List<ExerciseDto> Exercises { get; set; } = new List<ExerciseDto>();
    }

    [DataContract]
    public class ExerciseDto
    {
        [DataMember] public Guid Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int Sets { get; set; }
        [DataMember] public int Reps { get; set; }
        [DataMember] public double WeightKg { get; set; }
        [DataMember] public int DurationSeconds { get; set; }
    }

    [DataContract]
    public class UserDto
    {
        [DataMember] public Guid Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int AgeYears { get; set; }
        [DataMember] public double WeightKg { get; set; }
    }
}