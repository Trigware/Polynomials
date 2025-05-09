namespace Polynomials
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var term1 = new Term(3, 'x', 2);
            var term2 = new Term(5, 'y', 1);
            var term3 = new Term(7, 'z', 3);
            Polynomial poly = new(term3, term2, term1);
            Console.WriteLine(poly.Evaluate(('x', 20), ('y', 40), ('z', 15)));
        }
    }
    public struct Polynomial
    {
        public List<Term> Terms;
        public int TermCount { get => Terms.Count; }
        public Polynomial(params Term[] terms) => Terms = new(terms);
        public static implicit operator Polynomial(Term term) => new(term);
        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Terms.Count; i++)
            {
                Term term = Terms[i];
                if (i > 0 && term.Coefficient >= 0) result += "+";
                result += term.ToString();
            }
            return result;
        }
        public double Evaluate(char variable, double value) => Evaluate((variable, value));
        public double Evaluate(params (char variable, double value)[] substitutions)
        {
            double result = 0;
            foreach (Term term in Terms)
                result += term.EvaluateArray(substitutions);
            return result;
        }
        public Polynomial Sort()
        {
            return this;
        }
    }
    public struct Term
    {
        public double Coefficient;
        private object _subPolynomial;
        public object Subpolynomial
        {
            get => _subPolynomial;
            set
            {
                switch (value)
                {
                    case char setChar:
                        if (!char.IsLetter(setChar)) throw new Exception("Variable must be denoted as a letter!");
                        _subPolynomial = value;
                        break;
                    case Polynomial: _subPolynomial = value; break;
                    case string setString:
                        if (setString != "") goto default;
                        _subPolynomial = ""; break;
                    default: throw new Exception("The term subpolynomial can either be char, a polynomial or an empty string (constant)!");
                }
            }
        }
        public uint Power;
        public Term(double constant) => ConstructTerm(constant, null, 1);
        public Term(double coefficient, char variable, uint power = 1) => ConstructTerm(coefficient, variable, power);
        public Term(double coefficient, Polynomial subpolynomial, uint power = 1) => ConstructTerm(coefficient, subpolynomial, power);
        private void ConstructTerm(double coefficient, object subpolynomial, uint power)
        {
            Coefficient = coefficient;

            if (subpolynomial == null || coefficient == 0)
            {
                Subpolynomial = "";
                Power = 0;
                return;
            }

            Power = power;
            Subpolynomial = Power == 0 ? "" : subpolynomial;
        }
        public override string ToString()
        {
            string printedSubpolynomial = Subpolynomial.ToString(), printedCoefficient = Coefficient == 1 ? "" : Coefficient.ToString();
            if (Subpolynomial is Polynomial && Coefficient != 1)
                printedSubpolynomial = "(" + Subpolynomial.ToString() + ")";
            return Power switch
            {
                0 => Coefficient.ToString(),
                1 => $"{printedCoefficient}{printedSubpolynomial}",
                _ => $"{printedCoefficient}{printedSubpolynomial}^{Power}"
            };
        }
        public double QuickEval(double value)
        {
            return Subpolynomial switch
            {
                char variable => Evaluate(variable, value),
                Polynomial => throw new Exception("Cannot substitute for a polynomial!"),
                _ => Coefficient
            };
        }
        public double Evaluate(char variable, double value) => EvaluateArray(new (char variable, double value)[] { new(variable, value) });
        public double Evaluate(params (char variable, double value)[] substitutions) => EvaluateArray(substitutions);
        public double EvaluateArray((char variable, double value)[] substitutions)
        {
            return Subpolynomial switch
            {
                char termVariable => EvaluateVariable(termVariable, substitutions),
                Polynomial subPolynomial => Coefficient * Math.Pow(subPolynomial.Evaluate(substitutions), Power),
                _ => Coefficient
            };
        }
        private double EvaluateVariable(char termVariable, (char variable, double value)[] substitutions)
        {
            foreach ((char variable, double value) substitution in substitutions)
            {
                if (substitution.variable == termVariable)
                    return Coefficient * Math.Pow(substitution.value, Power);
            }
            throw new Exception($"There was a variable that you didn't give the substituted value for ({termVariable})!");
        }
    }
    public record struct Variable(char Denotation, uint Power);
}
