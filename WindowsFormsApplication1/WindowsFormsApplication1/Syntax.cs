using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;

namespace WindowsFormsApplication1
{
    public class Line
    {
        public Line(Point start, Point finish)
        {
            Start = start;
            Finish = finish;
        }

        public Point Start { get; set; }
        public Point Finish { get; set; }

        public void Scale(double xScale, double yScale, Point centerPoint)
        {
            Vector length = Finish - Start;
            Vector startDelta = Start - centerPoint;

            startDelta.X *= xScale;
            startDelta.Y *= yScale;
            Start = centerPoint + startDelta;
            length.X *= xScale;
            length.Y *= yScale;
            Finish = Start + length;
        }
        public void Shift(double xDelta, double yDelta)
        {
            var shift = new Vector(xDelta, yDelta);

            Finish += shift;
            Start += shift;
        }
    }

    public class Type
    {
        public Type(string name) { Name = name; }
        public String Name { get; private set; }
    }

    internal class TypeOfObjects : Type
    {
        private readonly Line standartElementLine;

        public TypeOfObjects(string name, Line standartElementLine) : base(name)
        {
            this.standartElementLine = standartElementLine;
        }

        public Objects Default
        {
            get { return new Objects(this, new Line(new Point(standartElementLine.Start.X, standartElementLine.Start.Y), new Point(standartElementLine.Finish.X, standartElementLine.Finish.Y))); }
        }
    }

    public class Objects
    {
        public Objects(Type type)
        {
            Type = type;
            Lines = new List<Line>();
        }
        public Objects(Type type, Line line)
        {
            Type = type;
            Lines = new List<Line> { line };
            StartPosition = new Point(Math.Min(line.Start.X, line.Finish.X),
                Math.Max(line.Start.Y, line.Finish.Y));
            EndPosition = new Point(Math.Max(line.Start.X, line.Finish.X),
                Math.Min(line.Start.Y, line.Finish.Y));
        }
        public Objects(Type type, IEnumerable<Line> lines, Point startPoint, Point endPoint)
        {
            StartPosition = startPoint;
            EndPosition = endPoint;
            Type = type;
            Lines = lines;
        }

        public Type Type { get; private set; }
        public Point StartPosition { get; set; }
        public Point EndPosition { get; set; }
        public IEnumerable<Line> Lines { get; private set; }

        public double Length
        {
            get { return Math.Abs(EndPosition.X - StartPosition.X); }
        }

        public double Height
        {
            get { return Math.Abs(EndPosition.Y - StartPosition.Y); }
        }

        public void Scale(double xScale, double yScale)
        {
            Vector delta = EndPosition - StartPosition;
            
            delta.X *= xScale;
            delta.Y *= yScale;
            EndPosition = StartPosition + delta;
            
            foreach (Line line in Lines)
                line.Scale(xScale, yScale, StartPosition);
        }
        public void Shift(double xDelta, double yDelta)
        {
            var shift = new Vector(xDelta, yDelta);
            
            StartPosition += shift;
            EndPosition += shift;
            
            foreach (Line line in Lines)
                line.Shift(xDelta, yDelta);
        }

        public void Draw(Graphics g)
        {
            var pen = new Pen(Color.Black);

            foreach (Line line in Lines)
            {
                var from = new System.Drawing.Point(Convert.ToInt32(GetScreenPoint(line.Start).X), Convert.ToInt32(GetScreenPoint(line.Start).Y));
                var to = new System.Drawing.Point(Convert.ToInt32(GetScreenPoint(line.Finish).X), Convert.ToInt32(GetScreenPoint(line.Finish).Y));
                g.DrawLine(pen, from, to);
            }
        }

        private Point GetScreenPoint(Point point)
        {
            return new Point(point.X, StartPosition.Y - point.Y);
        }
    }

    public abstract class Rule
    {
        protected Rule(Type startElementType, Type firstArgumentType, Type secondArgumentType)
        {
            SecondArgumentType = secondArgumentType;
            FirstArgumentType = firstArgumentType;
            StartElementType = startElementType;
            Random = new Random();
        }

        protected Random Random { get; private set; }
        public Type StartElementType { get; private set; }
        public Type FirstArgumentType { get; private set; }
        public Type SecondArgumentType { get; private set; }

        public abstract Objects TransformConnect(Objects first, Objects second);
        public abstract Objects Connect(Objects first, Objects second);
        public abstract bool IsRulePare(Objects first, Objects second);
    }

    public class InsideRule : Rule
    {
        private const int randomDelta = 3;

        public InsideRule(Type startElementType, Type firstArgumentType,
            Type secondArgumentType) : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Objects TransformConnect(Objects first, Objects second)
        {
            second.Scale(first.Length / second.Length + 0.8, first.Height / second.Height + 0.8);
            first.Shift(
                second.StartPosition.X +
                Random.Next((int)(Math.Abs(first.Length - second.Length) * 0.5), (int)(Math.Abs(first.Length - second.Length) * 0.8)) -
                first.StartPosition.X,
                second.EndPosition.Y +
                Random.Next((int)(Math.Abs(first.Height - second.Height) * 0.5), (int)(Math.Abs(first.Height - second.Height) * 0.8)) -
                first.EndPosition.Y
            );

            return Connect(first, second);
        }

        public override Objects Connect(Objects first, Objects second)
        {
            var resultLines = new List<Line>(first.Lines);
            
            resultLines.AddRange(second.Lines);
            
            var connect = new Objects(StartElementType, resultLines, second.StartPosition,
                second.EndPosition);
            
            return connect;
        }

        public override bool IsRulePare(Objects first, Objects second)
        {
            if (first.Type.Name != FirstArgumentType.Name ||
                second.Type.Name != SecondArgumentType.Name) 
                return false;

            return first.StartPosition.X > second.StartPosition.X - randomDelta &&
                   first.StartPosition.Y - randomDelta < second.StartPosition.Y
                   && first.EndPosition.X - randomDelta < second.EndPosition.X &&
                   first.EndPosition.Y > second.EndPosition.Y - randomDelta;
        }
    }

    public class LeftRule : Rule
    {
        private const int randomDelta = 3;

        public LeftRule(Type startElementType, Type firstArgumentType,
            Type secondArgumentType) : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Objects TransformConnect(Objects first, Objects second)
        {
            second.Shift(first.Length + Random.Next(1, 10), 0);
            return Connect(first, second);
        }

        public override Objects Connect(Objects first, Objects second)
        {
            var resultLines = new List<Line>(first.Lines);
            
            resultLines.AddRange(second.Lines);
            
            var startPosition = new Point(first.StartPosition.X,
                Math.Max(first.StartPosition.Y, second.StartPosition.Y));
            var endPosition = new Point(second.EndPosition.X,
                Math.Min(first.EndPosition.Y, second.EndPosition.Y));
            var connect = new Objects(StartElementType, resultLines, startPosition, endPosition);
            
            return connect;
        }

        public override bool IsRulePare(Objects first, Objects second)
        {
            if (first.Type.Name != FirstArgumentType.Name ||
                second.Type.Name != SecondArgumentType.Name) 
                return false;

            return first.EndPosition.X - randomDelta < second.StartPosition.X;
        }
    }

    public class UpRule : Rule
    {
        private const int randomDelta = 3;

        public UpRule(Type startElementType, Type firstArgumentType, Type secondArgumentType)
            : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Objects TransformConnect(Objects first, Objects second)
        {
            MakeSameLength(first, second);
            first.Shift(0, second.StartPosition.Y + Random.Next(0, 3));

            return Connect(first, second);
        }

        public override Objects Connect(Objects first, Objects second)
        {
            var resultLines = new List<Line>(first.Lines);

            resultLines.AddRange(second.Lines);

            var connect = new Objects(StartElementType, resultLines, first.StartPosition,
                second.EndPosition);

            return connect;
        }
        private static void MakeSameLength(Objects first, Objects second)
        {
            Objects largestElement = GetLargestElement(first, second);
            Objects shortestElement = GetShortestElement(first, second);

            shortestElement.Scale(largestElement.Length / shortestElement.Length, 1.0);
        }
        private static Objects GetLargestElement(Objects first, Objects second)
        {
            if (first.Length > second.Length)
                return first;
            else
                return second;
        }
        private static Objects GetShortestElement(Objects first, Objects second)
        {
            if (first.Length < second.Length)
                return first;
            else
                return second;
        }
        public override bool IsRulePare(Objects first, Objects second)
        {
            if (first.Type.Name != FirstArgumentType.Name ||
                second.Type.Name != SecondArgumentType.Name)
                return false;

            return second.StartPosition.Y - randomDelta < first.EndPosition.Y;
        }
    }

    public class Grammar
    {
        private static readonly Dictionary<string, Type> ElementTypes = GetElementTypes();
        private readonly List<Rule> rules;
        private readonly Type startElementType;

        public Grammar()
        {
            startElementType = new Type("face");

            rules = new List<Rule>
            {
                new LeftRule(ElementTypes["ear"], ElementTypes["a3"], ElementTypes["a4"]),
                new LeftRule(ElementTypes["ears"], ElementTypes["ear"], ElementTypes["ear"]),
                new LeftRule(ElementTypes["parallel"], ElementTypes["a4"], ElementTypes["a3"]),
                new UpRule(ElementTypes["square"], ElementTypes["ear"], ElementTypes["parallel"]),
                new UpRule(ElementTypes["eye"], ElementTypes["ear"], ElementTypes["a1"]),
                new LeftRule(ElementTypes["eyes"], ElementTypes["eye"], ElementTypes["eye"]),
                new UpRule(ElementTypes["eyesAndMouth"], ElementTypes["eyes"], ElementTypes["a1"]),
                new InsideRule(ElementTypes["head"], ElementTypes["eyesAndMouth"],ElementTypes["square"] ),
                new UpRule(startElementType, ElementTypes["ears"], ElementTypes["head"]),
            };
        }

        private static Dictionary<string, Type> GetElementTypes()
        {
            return new Dictionary<string, Type>
            {
                {"a1", new TypeOfObjects("a1", new Line(new Point(0, 0), new Point(10, 0)))},
                {"a2", new TypeOfObjects("a2", new Line(new Point(0, 0), new Point(0, 10)))},
                {"a3", new TypeOfObjects("a3", new Line(new Point(0, 0), new Point(10, 10)))},
                {"a4", new TypeOfObjects("a4", new Line(new Point(10, 0), new Point(0, 10)))},
                {"ear", new Type("ear")},
                {"ears", new Type("ears")},
                {"parallel", new Type("parallel")},
                {"square", new Type("square")},
                {"eyes", new Type("eyes")},
                {"eye", new Type("eye")},
                {"eyesAndMouth", new Type("eyesAndMouth")},
                {"head", new Type("head")},
            };
        }

        public Objects GetGrammar()
        {
            return GetElement(startElementType);
        }
        private Objects GetElement(Type elementType)
        {
            var terminalElementType = elementType as TypeOfObjects;

            if (terminalElementType != null)
                return terminalElementType.Default;

            Rule rule = rules.FirstOrDefault(x => x.StartElementType.Name == elementType.Name);

            return rule.TransformConnect(GetElement(rule.FirstArgumentType),
                GetElement(rule.SecondArgumentType));
        }
        public RecognazingResult IsAtGrammar(IEnumerable<Objects> baseElements)
        {
            var elements = new ConcurrentBag<Objects>(baseElements);

            for (int i = 0; i < rules.Count; i++)
            {
                ContainRuleAgrumentsResult result = ContainRuleAgruments(elements, rules[i]);
                elements = result.Elements;
                if (!result.IsElementFound)
                    return new RecognazingResult(rules[i].StartElementType.Name, false);
            }

            return new RecognazingResult("", true);
        }

        private static ContainRuleAgrumentsResult ContainRuleAgruments(ConcurrentBag<Objects> elements, Rule rule)
        {
            var result = new ContainRuleAgrumentsResult
            {
                Elements = new ConcurrentBag<Objects>(elements),
                IsElementFound = false
            };

            foreach (Objects firstElement in elements)
                if (firstElement.Type.Name == rule.FirstArgumentType.Name)
                    result = ContainRuleAgrumentsForFirstElement(elements, rule, firstElement, result);

            return result;
        }
        private static ContainRuleAgrumentsResult ContainRuleAgrumentsForFirstElement(IEnumerable<Objects> elements, Rule rule, Objects firstElement, ContainRuleAgrumentsResult result)
        {
            Objects element = firstElement;
            Parallel.ForEach(elements, (Objects secondElement) =>
            {
                if (rule.IsRulePare(element, secondElement))
                {
                    result.Elements.Add(rule.Connect(element, secondElement));
                    result.IsElementFound = true;
                }
            });

            return result;
        }

        public static Objects GetTerminalElement(Line line)
        {
            String resultName = GetTerminalElementName(line);
            return new Objects(ElementTypes[resultName], line);
        }
        private static string GetTerminalElementName(Line line)
        {
            double deltaX = line.Start.X - line.Finish.X;
            double deltaY = line.Start.Y - line.Finish.Y;

            if (Math.Abs(deltaY) < 1)
                return "a1";
            if (Math.Abs(deltaX) < 1)
                return "a2";
            if (Math.Abs(deltaX) < 1)
                return "a2";
            if (Math.Abs(deltaX / deltaY) < 0.2)
                return "a2";
            if (Math.Abs(deltaY / deltaX) < 0.2)
                return "a1";

            Point highPoint;
            if (line.Finish.Y > line.Start.Y)
                highPoint = line.Finish;
            else
                highPoint = line.Start;

            Point lowPoint;
            if (line.Finish.Y < line.Start.Y)
                lowPoint = line.Finish;
            else
                lowPoint = line.Start;

            if (highPoint.X < lowPoint.X)
                return "a4";

            return "a3";
        }  
    }

    public class ContainRuleAgrumentsResult
    {
        public ConcurrentBag<Objects> Elements { get; set; }
        public bool IsElementFound { get; set; }
    }

    public class RecognazingResult
    {
        public RecognazingResult(string errorElementName, bool isHome)
        {
            ErrorElementName = errorElementName;
            IsHome = isHome;
        }

        public String ErrorElementName { get; set; }
        public bool IsHome { get; set; }
    }
}
