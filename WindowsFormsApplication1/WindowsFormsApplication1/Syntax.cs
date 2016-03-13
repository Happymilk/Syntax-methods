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
        public Line(Point from, Point to)
        {
            From = from;
            To = to;
        }

        public Point From { get; set; }
        public Point To { get; set; }

        public void ScaleTransform(double xScale, double yScale, Point centerPoint)
        {
            Vector length = To - From;
            Vector startDelta = From - centerPoint;

            startDelta.X *= xScale;
            startDelta.Y *= yScale;
            From = centerPoint + startDelta;
            length.X *= xScale;
            length.Y *= yScale;
            To = From + length;
        }

        public void ShiftTransform(double xDelta, double yDelta)
        {
            var shift = new Vector(xDelta, yDelta);

            To += shift;
            From += shift;
        }
    }

    public class ElementType
    {
        public ElementType(string name) { Name = name; }
        public String Name { get; private set; }
    }

    internal class TerminalElementType : ElementType
    {
        private readonly Line standartElementLine;

        public TerminalElementType(string name, Line standartElementLine)
            : base(name)
        {
            this.standartElementLine = standartElementLine;
        }

        public Element StandartElement
        {
            get { return new Element(this, new Line(new Point(standartElementLine.From.X, standartElementLine.From.Y), new Point(standartElementLine.To.X, standartElementLine.To.Y))); }
        }
    }

    public class Element
    {
        public Element(ElementType elementType)
        {
            ElementType = elementType;
            Lines = new List<Line>();
        }

        public Element(ElementType elementType, Line line)
        {
            ElementType = elementType;
            Lines = new List<Line> { line };
            StartPosition = new Point(Math.Min(line.From.X, line.To.X),
                Math.Max(line.From.Y, line.To.Y));
            EndPosition = new Point(Math.Max(line.From.X, line.To.X),
                Math.Min(line.From.Y, line.To.Y));
        }

        public Element(ElementType elementType, IEnumerable<Line> lines, 
            Point startPoint, Point endPoint)
        {
            StartPosition = startPoint;
            EndPosition = endPoint;
            ElementType = elementType;
            Lines = lines;
        }

        public ElementType ElementType { get; private set; }
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

        public void ScaleTransform(double xScale, double yScale)
        {
            Vector delta = EndPosition - StartPosition;
            
            delta.X *= xScale;
            delta.Y *= yScale;
            EndPosition = StartPosition + delta;
            
            foreach (Line line in Lines)
                line.ScaleTransform(xScale, yScale, StartPosition);
        }

        public void ShiftTransform(double xDelta, double yDelta)
        {
            var shift = new Vector(xDelta, yDelta);
            
            StartPosition += shift;
            EndPosition += shift;
            
            foreach (Line line in Lines)
                line.ShiftTransform(xDelta, yDelta);
        }

        public void GetGeometryGroup(Graphics g)
        {
            var pen = new Pen(Color.Black);

            foreach (Line line in Lines)
            {
                var from = new System.Drawing.Point(Convert.ToInt32(GetScreenPoint(line.From).X), Convert.ToInt32(GetScreenPoint(line.From).Y));
                var to = new System.Drawing.Point(Convert.ToInt32(GetScreenPoint(line.To).X), Convert.ToInt32(GetScreenPoint(line.To).Y));
                g.DrawLine(pen, from, to);
            }
        }

        private Point GetScreenPoint(Point point)
        {
            return new Point(point.X, StartPosition.Y - point.Y);
        }
    }

    public class HomeGrammar
    {
        private static readonly Dictionary<string, ElementType> ElementTypes = GetElementTypes();
        private readonly List<Rule> rules;
        private readonly ElementType startElementType;

        public HomeGrammar()
        {
            startElementType = new ElementType("cat");

            rules = new List<Rule>
            {
                new LeftRule(ElementTypes["ear"], ElementTypes["a3"], ElementTypes["a4"]),
                new LeftRule(ElementTypes["ears"], ElementTypes["ear"], ElementTypes["ear"]),
                new LeftRule(ElementTypes["parallel"], ElementTypes["a4"], ElementTypes["a3"]),
                new UpRule(ElementTypes["square"], ElementTypes["ear"], ElementTypes["parallel"]),
                new UpRule(ElementTypes["eye"], ElementTypes["ear"], ElementTypes["a1"]),
                new LeftRule(ElementTypes["eyes"], ElementTypes["eye"], ElementTypes["eye"]),
                new UpRule(ElementTypes["eyesAndMouse"], ElementTypes["eyes"], ElementTypes["a1"]),
                new InsideRule(ElementTypes["fase"], ElementTypes["eyesAndMouse"],ElementTypes["square"] ),
                new UpRule(startElementType, ElementTypes["ears"], ElementTypes["fase"]),
            };
        }

        private static Dictionary<string, ElementType> GetElementTypes()
        {
            return new Dictionary<string, ElementType>
            {
                {"a1", new TerminalElementType("a1", new Line(new Point(0, 0), new Point(10, 0)))},
                {"a2", new TerminalElementType("a2", new Line(new Point(0, 0), new Point(0, 10)))},
                {"a3", new TerminalElementType("a3", new Line(new Point(0, 0), new Point(10, 10)))},
                {"a4", new TerminalElementType("a4", new Line(new Point(10, 0), new Point(0, 10)))},
                {"ear", new ElementType("ear")},
                {"ears", new ElementType("ears")},
                {"parallel", new ElementType("parallel")},
                {"square", new ElementType("square")},
                {"eyes", new ElementType("eyes")},
                {"eye", new ElementType("eye")},
                {"eyesAndMouse", new ElementType("eyesAndMouse")},
                {"fase", new ElementType("fase")},
            };
        }

        public Element GetHome()
        {
            return GetElement(startElementType);
        }

        private Element GetElement(ElementType elementType)
        {
            var terminalElementType = elementType as TerminalElementType;
            
            if (terminalElementType != null)
                return terminalElementType.StandartElement;

            Rule rule = rules.FirstOrDefault(x => x.StartElementType.Name == elementType.Name);
            
            return rule.TransformConnect(GetElement(rule.FirstArgumentType),
                GetElement(rule.SecondArgumentType));
        }

        public RecognazingResult IsHome(IEnumerable<Element> baseElements)
        {
            var elements = new ConcurrentBag<Element>(baseElements);
            
            for (int i = 0; i < rules.Count; i++)
            {
                ContainRuleAgrumentsResult result = ContainRuleAgruments(elements, rules[i]);
                elements = result.Elements;
                if (!result.IsElementFound)
                    return new RecognazingResult(rules[i].StartElementType.Name, false);
            }

            return new RecognazingResult("", true);
        }

        private static ContainRuleAgrumentsResult ContainRuleAgruments(
            ConcurrentBag<Element> elements, Rule rule)
        {
            var result = new ContainRuleAgrumentsResult
            {
                Elements = new ConcurrentBag<Element>(elements),
                IsElementFound = false
            };

            foreach (Element firstElement in elements)
                if (firstElement.ElementType.Name == rule.FirstArgumentType.Name)
                    result = ContainRuleAgrumentsForFirstElement(elements, rule, firstElement, result);
            
            return result;
        }

        private static ContainRuleAgrumentsResult ContainRuleAgrumentsForFirstElement(
            IEnumerable<Element> elements, Rule rule,
            Element firstElement, ContainRuleAgrumentsResult result)
        {
            Element element = firstElement;
            Parallel.ForEach(elements, (Element secondElement) =>
            {
                if (rule.IsRulePare(element, secondElement))
                {
                    result.Elements.Add(rule.Connect(element, secondElement));
                    result.IsElementFound = true;
                }
            });

            return result;
        }

        public static Element GetTerminalElement(Line line)
        {
            String resultName = GetTerminalElementName(line);
            return new Element(ElementTypes[resultName], line);
        }

        private static string GetTerminalElementName(Line line)
        {
            double deltaX = line.From.X - line.To.X;
            double deltaY = line.From.Y - line.To.Y;

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
            if (line.To.Y > line.From.Y)
                highPoint = line.To;
            else
                highPoint = line.From;

            Point lowPoint;
            if (line.To.Y < line.From.Y)
                lowPoint = line.To;
            else
                lowPoint = line.From;

            if (highPoint.X < lowPoint.X) 
                return "a4";
            
            return "a3";
        }

        private class ContainRuleAgrumentsResult
        {
            public ConcurrentBag<Element> Elements { get; set; }
            public bool IsElementFound { get; set; }
        }
    }

    public abstract class Rule
    {
        protected Rule(ElementType startElementType, ElementType firstArgumentType,
            ElementType secondArgumentType)
        {
            SecondArgumentType = secondArgumentType;
            FirstArgumentType = firstArgumentType;
            StartElementType = startElementType;
            Random = new Random();
        }

        protected Random Random { get; private set; }
        public ElementType StartElementType { get; private set; }
        public ElementType FirstArgumentType { get; private set; }
        public ElementType SecondArgumentType { get; private set; }

        public abstract Element TransformConnect(Element first, Element second);
        public abstract Element Connect(Element first, Element second);
        public abstract bool IsRulePare(Element first, Element second);
    }

    public class InsideRule : Rule
    {
        private const int randomDelta = 3;

        public InsideRule(ElementType startElementType, ElementType firstArgumentType,
            ElementType secondArgumentType) : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Element TransformConnect(Element first, Element second)
        {
            second.ScaleTransform(first.Length / second.Length + 0.8, first.Height / second.Height + 0.8);
            first.ShiftTransform(
                second.StartPosition.X +
                Random.Next((int)(Math.Abs(first.Length - second.Length) * 0.5), (int)(Math.Abs(first.Length - second.Length) * 0.8)) -
                first.StartPosition.X,
                second.EndPosition.Y +
                Random.Next((int)(Math.Abs(first.Height - second.Height) * 0.5), (int)(Math.Abs(first.Height - second.Height) * 0.8)) -
                first.EndPosition.Y
            );

            return Connect(first, second);
        }

        public override Element Connect(Element first, Element second)
        {
            var resultLines = new List<Line>(first.Lines);
            
            resultLines.AddRange(second.Lines);
            
            var connect = new Element(StartElementType, resultLines, second.StartPosition,
                second.EndPosition);
            
            return connect;
        }

        public override bool IsRulePare(Element first, Element second)
        {
            if (first.ElementType.Name != FirstArgumentType.Name ||
                second.ElementType.Name != SecondArgumentType.Name) 
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

        public LeftRule(ElementType startElementType, ElementType firstArgumentType,
            ElementType secondArgumentType) : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Element TransformConnect(Element first, Element second)
        {
            second.ShiftTransform(first.Length + Random.Next(1, 10), 0);
            return Connect(first, second);
        }

        public override Element Connect(Element first, Element second)
        {
            var resultLines = new List<Line>(first.Lines);
            
            resultLines.AddRange(second.Lines);
            
            var startPosition = new Point(first.StartPosition.X,
                Math.Max(first.StartPosition.Y, second.StartPosition.Y));
            var endPosition = new Point(second.EndPosition.X,
                Math.Min(first.EndPosition.Y, second.EndPosition.Y));
            var connect = new Element(StartElementType, resultLines, startPosition, endPosition);
            
            return connect;
        }

        public override bool IsRulePare(Element first, Element second)
        {
            if (first.ElementType.Name != FirstArgumentType.Name ||
                second.ElementType.Name != SecondArgumentType.Name) 
                return false;

            return first.EndPosition.X - randomDelta < second.StartPosition.X;
        }
    }

    public class UpRule : Rule
    {
        private const int randomDelta = 3;

        public UpRule(ElementType startElementType, ElementType firstArgumentType,
            ElementType secondArgumentType)
            : base(startElementType, firstArgumentType, secondArgumentType) { }

        public override Element TransformConnect(Element first, Element second)
        {
            MakeSameLength(first, second);
            first.ShiftTransform(0, second.StartPosition.Y + Random.Next(0, 3));

            return Connect(first, second);
        }

        public override Element Connect(Element first, Element second)
        {
            var resultLines = new List<Line>(first.Lines);

            resultLines.AddRange(second.Lines);

            var connect = new Element(StartElementType, resultLines, first.StartPosition,
                second.EndPosition);

            return connect;
        }

        private static void MakeSameLength(Element first, Element second)
        {
            Element largestElement = GetLargestElement(first, second);
            Element shortestElement = GetShortestElement(first, second);

            shortestElement.ScaleTransform(largestElement.Length / shortestElement.Length, 1.0);
        }

        private static Element GetLargestElement(Element first, Element second)
        {
            if (first.Length > second.Length)
                return first;
            else
                return second;
        }

        private static Element GetShortestElement(Element first, Element second)
        {
            if (first.Length < second.Length)
                return first;
            else
                return second;
        }

        public override bool IsRulePare(Element first, Element second)
        {
            if (first.ElementType.Name != FirstArgumentType.Name ||
                second.ElementType.Name != SecondArgumentType.Name)
                return false;

            return second.StartPosition.Y - randomDelta < first.EndPosition.Y;
        }
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
