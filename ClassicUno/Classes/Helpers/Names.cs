/* Classic Uno 2026
 * Version: 1.0
 * By: Jason James Newland
 * ©2026 - Kangasoft Software */
using System;
using System.Collections.Generic;

namespace ClassicUno.Classes.Helpers
{
    public enum PlayerGender
    {
        Male = 0,
        Female = 1,
        NonBinary = 2
    }

    [Serializable]
    public class PlayerNameData
    {
        public string Name { get; set; }

        public PlayerGender Gender { get; set; }
    }

    public static class Names
    {
        private static readonly Random Gender = new Random();

        private static readonly Random Name = new Random();

        private static readonly List<string> MaleNames = new List<string>()
        {
            "John", "Tim", "Sam", "Bill", "Henry", "Paul", "Michael", "Mitch", "Steve", "William", "Martin", "Colin", "Allen"
        };

        private static readonly List<string> FemaleNames = new List<string>()
        {
            "Samantha", "Diane", "Julie", "Annie", "Amy", "Jenny", "Alica", "Jasmine", "Wendy", "Rebecca", "Claire", "Amanda"
        };

        public static PlayerNameData GetRandomName()
        {
            PlayerGender gender;
            List<string> names;
            /* Choose a random gender */
            switch (Gender.Next(0, 2))
            {
                case 0:
                    gender = PlayerGender.Male;
                    names = MaleNames;
                    break;

                default:
                    gender = PlayerGender.Female;
                    names = FemaleNames;
                    break;
            }
            /* Pick a random name based on gender */
            var name = names[Name.Next(0, names.Count - 1)];
            var data = new PlayerNameData
            {
                Name = name,
                Gender = gender
            };
            return data;
        }
    }
}
