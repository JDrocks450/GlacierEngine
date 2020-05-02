using AntFarm.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public class Chromosome
    {
        const PersonalityTraits DominantTrait = PersonalityTraits.Peaceful;
        public enum PersonalityTraits
        {
            Peaceful = 0,
            Aggressive = 1
        }
        public PersonalityTraits Trait1, Trait2;
        public Chromosome(PersonalityTraits one, PersonalityTraits two)
        {
            Trait1 = one;
            Trait2 = two;
        }

        public static Chromosome GetRandomGenetics()
        {
            return new Chromosome((PersonalityTraits)GameResources.Rand.Next(0, 2), (PersonalityTraits)GameResources.Rand.Next(0, 2));
        }

        public static Chromosome Procreate(Chromosome one, Chromosome two)
        {
            Chromosome[] punnetSquare = new Chromosome[]
            {
                new Chromosome(one.Trait1, two.Trait1), //AA
                new Chromosome(one.Trait1, two.Trait2), //AB
                new Chromosome(one.Trait2, two.Trait1), //BA
                new Chromosome(one.Trait2, two.Trait2), //BB
            };
            return punnetSquare[GameResources.Rand.Next(0, 4)];
        }

        public PersonalityTraits GetExhibitedTrait()
        {
            if (Trait1 == Trait2)
                return Trait1;
            if (Trait1 == DominantTrait)
                return DominantTrait; // dominant trait: Peaceful
            return PersonalityTraits.Aggressive; //shouldn't happen?
        }
    }
}
