// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Provides a generic data structure for a collection that has been paged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedList<T> : List<T> where T : class
    {
        private int m_totalcount;
        public int TotalCount
        {
            get { return m_totalcount; }
            set { m_totalcount = value; }
        }
        private int m_pagesize;
        public int PageSize
        {
            get { return m_pagesize; }
            set { m_pagesize = value; }
        }
        private int m_currentpage;
        public int CurrentPage
        {
            get { return m_currentpage; }
            set { m_currentpage = value; }
        }
        private int m_totalpages;
        public int TotalPages
        {
            get { return m_totalpages; }
            set { m_totalpages = value; }
        }
        private int m_startindex;
        public int StartIndex
        {
            get { return m_startindex; }
            set { m_startindex = value; }
        }
        private int m_endindex;
        public int EndIndex
        {
            get { return m_endindex; }
            set { m_endindex = value; }
        }
    }

    /// <summary>
    /// Provides a generic search template for search collection and returns
    /// a paginated list based on the original collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedSearchTemplate<T> where T : class
    {
        public PagedSearchTemplate()
        {
            Filters = new List<Func<T, bool>>();
        }
        /// <summary>
        /// Gets or sets the delegate method to do filter
        /// </summary>
        public List<Func<T, bool>> Filters { get; private set; }

        private Func<IEnumerable<T>, IEnumerable<T>> m_sorter;

        public Func<IEnumerable<T>, IEnumerable<T>> Sorter
        {
            get { return m_sorter; }
            set { m_sorter = value; }
        }

        /// <summary>
        /// Does the search routine
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public PaginatedList<T> Search(IEnumerable<T> source, int pageSize, int currentPage)
        {
            PaginatedList<T> pagedList = new PaginatedList<T>();
            IEnumerable<T> results = source;

            // apply filter
            if (Filters.Count > 0)
            {
                // apply search filter
                foreach (Func<T, bool> filter in Filters)
                {
                    results = results.Where(filter);
                }
            }

            // if a sort function was provided, sort it first before paging it
            if (Sorter != null)
            {
                results = Sorter(results);
            }
            
            // Determine if paging was required by checking the pageSize
            // pagesize = 1 means display all
            // If pagesize was specified, we do paging on the datasource collection
            // and compute the paging info for our returned data structure
            if (pageSize > 1)
            {
                int count = results.Count();

                if (count <= pageSize)
                {
                    pageSize = count;
                    currentPage = 1;
                }

                if (count > 0 && pageSize > 0)
                {
                    if (count % pageSize == 0)
                    {
                        if (currentPage > (count / pageSize))
                        {
                            currentPage = 1;
                        }
                    }
                    else if (count % pageSize > 0)
                    {
                        if (currentPage > ((count / pageSize) + 1))
                        {
                            currentPage = 1;
                        }
                    }
                }
                

                // determine where to start
                int startAt = (currentPage - 1) * pageSize;

                // now finally get the results
                results = results.Skip(startAt).Take(pageSize);

                // inject the raw collectionn in our paginated data structure
                pagedList.AddRange(results);

                pagedList.TotalCount = count;
                pagedList.PageSize = pageSize;

                if (count > 0)
                {
                    pagedList.TotalPages = count / pageSize;

                    int remainder = count % pageSize;
                    if (remainder > 0)
                    {
                        pagedList.TotalPages += 1;
                    }

                    pagedList.CurrentPage = currentPage; // selected page...

                    pagedList.StartIndex = startAt + 1; // start at 1 not 0
                    pagedList.EndIndex = startAt + pageSize;
                }
            }
            else
            {
                // no paging provided, probably just search or alpha filter
                // now just extract the results
                pagedList.AddRange(results);
            }

            return pagedList;
        }
    }
}









