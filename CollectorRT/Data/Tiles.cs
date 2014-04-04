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
            else if (source.Kind == "twitter" || source.Kind == "twitter-user")
            {
                tile = "Background-Twitter.png";
            }
            else if (source.Kind == "tumblr")
            {
                tile = "Background-Tumblr.png";
            }

            return new Uri(path + tile, UriKind.Absolute);
        }

        public static Color ColorForSource(Source source)
        {
            if (source.Kind == "twitter" || source.Kind == "twitter-user")
            {
                return Color.FromArgb(255, 0, 172, 237);
            }
            else if (source.Kind == "tumblr")
            {
                return Color.FromArgb(255, 52, 80, 107);
            }

            return Color.FromArgb(255, 188, 45, 48);
        }
    }
}
