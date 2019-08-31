using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StarryNight.Model
{
    class BeeStarModel
    {
        public static readonly Size StarSize = new Size(150, 100);

        private readonly Dictionary<Bee, Point> _bees = new Dictionary<Bee, Point>();
        private readonly Dictionary<Star, Point> _stars = new Dictionary<Star, Point>();
        private Random _random = new Random();

        private Size _playAreaSize;

        public BeeStarModel()
        {
            _playAreaSize = Size.Empty;
        }

        public void Update()
        {
            MoveOneBee();
            AddOrRemoveAStar();
        }

        private static bool RectsOverlap(Rect r1, Rect r2)
        {
            r1.Intersect(r2);
            if (r1.Width > 0 || r1.Height > 0)
                return true;
            return false;
        }

        public Size PlayAreaSize
        {
            get
            {
                return _playAreaSize;
            }
            set
            {
                _playAreaSize = value;
                CreateBees();
                CreateStars();
            }
        }
        private void CreateBees()
        {
            if (PlayAreaSize == Size.Empty)
                return;
            if (_bees.Count() > 0)
            {
                List<Bee> allBees = _bees.Keys.ToList();
                foreach (Bee bee in allBees)
                    MoveOneBee(bee);
            }                
            else
            {
                int beeCount = _random.Next(5, 15);
                for (int i = 0; i < beeCount; i++)
                {
                    double s = _random.Next(40, 150);
                    Size beeSize = new Size(s, s);
                    Point beeLocation = FindNonOverlappingPoint(beeSize);
                    Bee newBee = new Bee(beeLocation, beeSize);
                    _bees[newBee] = new Point(beeLocation.X, beeLocation.Y);
                    OnBeeMoved(newBee, beeLocation.X, beeLocation.Y);
                }
            }
               
        }

        private void CreateStars()
        {
            if (_playAreaSize == Size.Empty)
                return;
            int starCount = _stars.Count();
            if (starCount > 0)
            {
                foreach (Star star in _stars.Keys)
                {
                    star.Location = FindNonOverlappingPoint(StarSize);
                    OnStarChanged(star, false);
                }

            }
            else
            {
                int randomStarCount = _random.Next(5, 10);
                for (int i = 0; i < randomStarCount; i++)
                    CreateAStar();
            }
        }

        private void CreateAStar()
        {
            Point newPoint = FindNonOverlappingPoint(StarSize);
            Star newStar = new Star(newPoint);
            OnStarChanged(newStar, false);
        }
        private Point FindNonOverlappingPoint(Size size)
        {
            Rect newRect = new Rect();
            bool noOverlap = false;
            int count = 0;
            while (!noOverlap)
            {
                newRect = new Rect(_random.Next((int)PlayAreaSize.Width - 150), _random.Next((int)PlayAreaSize.Height - 100), 
                    size.Width, size.Height);
                var overlappingBees = from bee in _bees.Keys
                                   where RectsOverlap(newRect, bee.Position)
                                   select bee;
                var overlappingStars = from star in _stars.Keys
                                     where RectsOverlap(newRect, new Rect(star.Location, StarSize))
                                     select star;
                if (overlappingBees.Count() > 0 || overlappingStars.Count() > 0 || count > 1000)
                    noOverlap = true;
            }
            return new Point(newRect.X, newRect.Y);
        }

        private void MoveOneBee(Bee bee = null)
        {
            int beesCount = _bees.Keys.Count();
            if (beesCount == 0)
                return;
            if (bee == null)
            {
                List<Bee> beesList = _bees.Keys.ToList();
                int randomNumber = _random.Next(beesCount);
                bee = beesList[randomNumber];
            }
            Point newLocation = FindNonOverlappingPoint(bee.Size);
            bee.Location = newLocation;
            _bees[bee] = bee.Location;
            OnBeeMoved(bee, newLocation.X, newLocation.Y);

        }

        private void AddOrRemoveAStar()
        {
            if ((_random.Next(2) == 0 || _stars.Keys.Count() <= 5) && _stars.Keys.Count() < 20)
                CreateAStar();
            else
            {
                Star starToRemove = _stars.Keys.ToList()[_random.Next(_stars.Count)];
                _stars.Remove(starToRemove);
                OnStarChanged(starToRemove, true);
            }          
        }

        public EventHandler<BeeMovedEventArgs> BeeMoved;
        private void OnBeeMoved(Bee beeThatMoved, double x, double y)
        {
            EventHandler<BeeMovedEventArgs> beeMoved = BeeMoved;
            if (beeMoved != null)
                beeMoved(this, new BeeMovedEventArgs(beeThatMoved, x, y));
        }

        public EventHandler<StarChangedEventArgs> StarChanged;
        private void OnStarChanged(Star starThatChanged, bool removed)
        {
            EventHandler<StarChangedEventArgs> starChanged = StarChanged;
            if (starChanged != null)
                starChanged(this, new StarChangedEventArgs(starThatChanged, removed));
        }
    }
}
