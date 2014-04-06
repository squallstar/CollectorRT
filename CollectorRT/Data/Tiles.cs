using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace CollectorRT.Data
{
    class Tiles
    {
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        /// <summary>
        /// Returns the path of the image for a given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static Uri ImageForSource(Source source)
        {
            string path = "ms-appx:///Assets/Categories/";
            string tile = "";

            if (source.Kind == "rss")
            {

                switch (source.Title.ToLower())
                {
                    case "art & design":
                    case "art":
                    case "design":
                        tile = "Background-ArtDesign.png";
                        break;

                    case "business":
                        tile = "Background-Business.png";
                        break;

                    case "cars & motocycles":
                        tile = "Background-Cars.png";
                        break;

                    case "decor and architecture":
                        tile = "Background-DecorArchitecture.png";
                        break;

                    case "entertainment":
                        tile = "Background-Entertainment.png";
                        break;

                    case "fashion & blog":
                    case "fashion":
                        tile = "Background-Fashion.png";
                        break;

                    case "food":
                    case "cooking":
                        tile = "Background-Food.png";
                        break;

                    case "graphic & web":
                    case "web":
                    case "graphic":
                    case "internet":
                        tile = "Background-Web.Graphic.png";
                        break;

                    case "health and beauty":
                        tile = "Background-Health.Beauty.png";
                        break;

                    case "lifestyle":
                    case "lifestyles":
                    case "life style":
                        tile = "Background-LifeStyle.png";
                        break;

                    case "man":
                    case "men":
                    case "men's fashion":
                        tile = "Background-ManStyle.png";
                        break;

                    case "music & shows":
                    case "music":
                    case "musics":
                    case "musica":
                        tile = "Background-Music.png";
                        break;

                    case "photography & video":
                    case "photography":
                    case "fotografia":
                        tile = "Background-PhotoeVideo.png";
                        break;

                    case "science":
                    case "science & technology":
                    case "phone":
                    case "windows":
                    case "microsoft":
                    case "tecnologia":
                    case "tecnology":
                        tile = "Background-Scie.Tech.png";
                        break;

                    case "sport":
                    case "sports":
                    case "soccer":
                    case "football":
                        tile = "Background-Sport.png";
                        break;

                    case "travel":
                    case "trip":
                    case "viaggi":
                        tile = "Background-Travel.png";
                        break;

                    case "videogames":
                    case "videogiochi":
                    case "games":
                    case "xbox":
                    case "xbox 360":
                    case "playstation":
                    case "video games":
                        tile = "Background-Videogame.png";
                        break;

                    case "world news":
                    case "news":
                    case "notizie":
                        tile = "Background-WorldNews.png";
                        break;

                    default:
                        tile = "Background-Article.png";
                        break;
                }
            }
            else if (source.IsTwitterKind)
            {
                tile = "Background-Twitter.png";
            }
            else if (source.IsTumblrKind)
            {
                tile = "Background-Tumblr.png";
            }

            return new Uri(path + tile, UriKind.Absolute);
        }

        public static Color ColorForSource(Source source)
        {
            if (source.IsTwitterKind)
            {
                return Color.FromArgb(255, 0, 172, 237);
            }
            else if (source.IsTumblrKind)
            {
                return Color.FromArgb(255, 52, 80, 107);
            }

            return GetRandomColor();
        }

        private static Color GetRandomColor()
        {
            Color[] c = new Color[16];
            c[0] = Color.FromArgb(255, 188, 45, 48);
            c[1] = Color.FromArgb(255, 0, 193, 197);
            c[2] = Color.FromArgb(255, 0, 151, 197);
            c[3] = Color.FromArgb(255, 197, 74, 0);
            c[4] = Color.FromArgb(255, 181, 197, 0);
            c[5] = Color.FromArgb(255, 0, 90, 197);
            c[6] = Color.FromArgb(255, 93, 0, 197);
            c[7] = Color.FromArgb(255, 197, 0, 155);
            c[8] = Color.FromArgb(255, 197, 0, 78);
            c[9] = Color.FromArgb(255, 254, 87, 90);
            c[10] = Color.FromArgb(255, 105, 206, 174);
            c[11] = Color.FromArgb(255, 83, 109, 185);
            c[12] = Color.FromArgb(255, 193, 213, 114);
            c[13] = Color.FromArgb(255, 96, 174, 188);
            c[14] = Color.FromArgb(255, 102, 96, 188);
            c[15] = Color.FromArgb(255, 241, 93, 74);

            return c[RandomNumber(0, c.Length)];
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
    }
}
