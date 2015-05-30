using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Extensions;
using ScottIsAFool.WindowsPhone.Extensions;
using Squirrel.Model;

namespace Squirrel.Extensions
{
    public static class SortExtensions
    {
        public static async Task<ObservableCollection<ExtendedPocketItem>> Sort(this IList<ExtendedPocketItem> list, SortType sortType)
        {
            var result = new ObservableCollection<ExtendedPocketItem>();

            if (list.IsNullOrEmpty())
            {
                return result;
            }

            await Task.Run(() =>
            {
                switch (sortType)
                {
                    case SortType.DateAscending:
                        result = list.OrderBy(x => x.AddTime).ToObservableCollection();
                        break;
                    case SortType.DateDescending:
                        result = list.OrderByDescending(x => x.AddTime).ToObservableCollection();
                        break;
                    case SortType.TitleAscending:
                        result = list.OrderBy(x => x.Title).ToObservableCollection();
                        break;
                    case SortType.TitleDescending:
                        result = list.OrderByDescending(x => x.Title).ToObservableCollection();
                        break;
                }
            });

            return result;
        }
    }
}
