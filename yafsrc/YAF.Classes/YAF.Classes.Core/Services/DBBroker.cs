/* Yet Another Forum.net
 * Copyright (C) 2006-2009 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
using System;
using System.Data;
using YAF.Classes;
using YAF.Classes.Data;
using YAF.Classes.Utils;

namespace YAF.Classes.Core
{
	/// <summary>
	/// Class used for multi-step DB operations so they can be cached, etc.
	/// </summary>
	public class YafDBBroker
	{
		/// <summary>
		/// Returns the layout of the board
		/// </summary>
		/// <param name="boardID">BoardID</param>
		/// <param name="userID">UserID</param>
		/// <param name="categoryID">CategoryID</param>
		/// <param name="parentID">ParentID</param>
		/// <returns>Returns board layout</returns>
		public DataSet BoardLayout( object boardID, object userID, object categoryID, object parentID )
		{
			if ( categoryID != null && long.Parse( categoryID.ToString() ) == 0 )
				categoryID = null;

			using ( DataSet ds = new DataSet() )
			{
				// get the cached version of forum moderators if it's valid
				string key = YafCache.GetBoardCacheKey( Constants.Cache.ForumModerators );
				DataTable moderator = YafContext.Current.Cache [key] as DataTable;

				// was it in the cache?
				if ( moderator == null )
				{
					// get fresh values
					moderator = DB.forum_moderators();
					moderator.TableName = YafDBAccess.GetObjectName( "Moderator" );

					// cache it for the time specified by admin
					YafContext.Current.Cache.Add(key, moderator, null, DateTime.Now.AddMinutes(YafContext.Current.BoardSettings.BoardModeratorsCacheTimeout), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Default, null);
				}
				// insert it into this DataSet
				ds.Tables.Add( moderator.Copy() );

				// get the Category Table
				key = YafCache.GetBoardCacheKey( Constants.Cache.ForumCategory );
				DataTable category = YafContext.Current.Cache [key] as DataTable;

				// was it in the cache?
				if ( category == null )
				{
					// just get all categories since the list is short
					category = DB.category_list( boardID, null );
					category.TableName = YafDBAccess.GetObjectName( "Category" );
					YafContext.Current.Cache.Add(key, category, null, DateTime.Now.AddMinutes(YafContext.Current.BoardSettings.BoardCategoriesCacheTimeout), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Default, null);
				}	

				// add it to this dataset				
				ds.Tables.Add( category.Copy() );

				if ( categoryID != null )
				{
					// make sure this only has the category desired in the dataset
					foreach ( DataRow row in ds.Tables [YafDBAccess.GetObjectName("Category")].Rows )
					{
						if ( Convert.ToInt32( row ["CategoryID"] ) != Convert.ToInt32(categoryID) )
						{
							// delete it...
							row.Delete();
						}
					}
					ds.Tables [YafDBAccess.GetObjectName( "Category" )].AcceptChanges();
				}

				DataTable forum = DB.forum_listread( boardID, userID, categoryID, parentID );
				forum.TableName = YafDBAccess.GetObjectName( "Forum" );
				ds.Tables.Add( forum.Copy() );

				ds.Relations.Add( "FK_Forum_Category", ds.Tables [YafDBAccess.GetObjectName( "Category" )].Columns ["CategoryID"], ds.Tables [YafDBAccess.GetObjectName( "Forum" )].Columns ["CategoryID"], false );
				ds.Relations.Add( "FK_Moderator_Forum", ds.Tables [YafDBAccess.GetObjectName( "Forum" )].Columns ["ForumID"], ds.Tables [YafDBAccess.GetObjectName( "Moderator" )].Columns ["ForumID"], false );

				bool deletedCategory = false;

				// remove empty categories...
				foreach ( DataRow row in ds.Tables[YafDBAccess.GetObjectName( "Category" )].Rows )
				{
					DataRow[] childRows = row.GetChildRows( "FK_Forum_Category" );

					if ( childRows.Length == 0 )
					{
						// remove this category...
						row.Delete();
						deletedCategory = true;
					}
				}

				if ( deletedCategory ) ds.Tables[YafDBAccess.GetObjectName( "Category" )].AcceptChanges();

				return ds;
			}
		}
	}
}