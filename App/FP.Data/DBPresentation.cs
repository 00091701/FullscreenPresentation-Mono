/*
 * This file is part of Fullscreen-Presentation
 * Copyright (C) 2012 David Hoffmann
 *
 * Fullscreen-Presentation is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, version 2.
 *
 * Fullscreen-Presentation is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AFullscreen-Presentation; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */


using System;
using System.Collections.Generic;
using De.Dhoffmann.Mono.FullscreenPresentation.Droid.Libs.FP.Data.Types;
using Mono.Data.Sqlite;
using System.Data.Common;
using De.Dhoffmann.Mono.FullscreenPresentation.Buslog;

namespace De.Dhoffmann.Mono.FullscreenPresentation.Data
{
	public class DBPresentation : DataBase
	{
		public DBPresentation ()
		{
		}

		public List<Presentation> Select(Guid presentationUID)
		{
			List<Presentation> ret = new List<Presentation>();

			string sSqlCmd = "SELECT PresentationUID, Name, DateCreate, Type FROM presentations ";

			if (presentationUID != Guid.Empty)
				sSqlCmd += "WHERE PresentationUID=@presentationUID ";

			sSqlCmd += "ORDER BY Name;";

			using (SqliteConnection conn = GetConnection())
			{
				using (SqliteCommand sqlCmd = new SqliteCommand(sSqlCmd, conn))
				{
					if (presentationUID != Guid.Empty)
						sqlCmd.Parameters.AddWithValue("@presentationUID", presentationUID.ToString());
					
					conn.Open();
					
					try
					{
						using (DbDataReader reader = sqlCmd.ExecuteReader())
						{
							while (reader.Read())
							{
								if (reader.HasRows)
								{
									if (reader.HasRows)
									{
										Presentation presentation = new Presentation();
										presentation.PresentationUID = new Guid(reader.GetString(0));
										presentation.Name = reader.GetString(1);
										presentation.DateCreate = reader.GetDateTime(2);
										presentation.Type = (Presentation.Typ)reader.GetInt32(3);

										ret.Add(presentation);
									}
								}
							}
						}
					}
					catch (SqliteException ex)
					{
						Logging.Log(this, Logging.LoggingTypeError, "SQL cmd: " + sqlCmd.ToString(), ex);
					}
					
					conn.Close();
				}
			}

			return ret;
		}

		public List<Presentation> Select(string name = null)
		{
			List<Presentation> ret = new List<Presentation>();
			
			string sSqlCmd = "SELECT PresentationUID, Name, DateCreate, Type FROM presentations ";
			
			if (!String.IsNullOrEmpty(name))
				sSqlCmd += "WHERE Name=@name ";
			
			sSqlCmd += "ORDER BY Name;";
			
			using (SqliteConnection conn = GetConnection())
			{
				using (SqliteCommand sqlCmd = new SqliteCommand(sSqlCmd, conn))
				{
					if (!String.IsNullOrEmpty(name))
						sqlCmd.Parameters.AddWithValue("@name", name);
					
					conn.Open();
					
					try
					{
						using (DbDataReader reader = sqlCmd.ExecuteReader())
						{
							while (reader.Read())
							{
								if (reader.HasRows)
								{
									if (reader.HasRows)
									{
										Presentation presentation = new Presentation();
										presentation.PresentationUID = new Guid(reader.GetString(0));
										presentation.Name = reader.GetString(1);
										presentation.DateCreate = reader.GetDateTime(2);
										presentation.Type = (Presentation.Typ)reader.GetInt32(3);
										
										ret.Add(presentation);
									}
								}
							}
						}
					}
					catch (SqliteException ex)
					{
						Logging.Log(this, Logging.LoggingTypeError, "SQL cmd: " + sqlCmd.ToString(), ex);
					}
					
					conn.Close();
				}
			}
			
			return ret;
		}

		public bool Insert(Presentation presentation)
		{
			bool ret = false;

			if (presentation == null || presentation.PresentationUID == Guid.Empty || String.IsNullOrEmpty(presentation.Name))
				return false;
			
			using (SqliteConnection conn = GetConnection())
			{
				using (SqliteCommand sqlCmd = new SqliteCommand(@"
					BEGIN; " +
                    "INSERT INTO presentations " +
                    "(PresentationUID, Name, DateCreate, Type) " +
                    "VALUES " +
				    "(@PresentationUID, @Name, @DateCreate, @Type); " +
					"COMMIT;", conn))
				{
					sqlCmd.Parameters.AddWithValue("@PresentationUID", presentation.PresentationUID.ToString());
					sqlCmd.Parameters.AddWithValue("@Name", presentation.Name);
					sqlCmd.Parameters.AddWithValue("@DateCreate", DateTime.Now);
					sqlCmd.Parameters.AddWithValue("@Type", (int)presentation.Type);

					conn.Open();

					try
					{
						sqlCmd.ExecuteNonQuery();
						ret = true;
					}
					catch (SqliteException ex)
					{
						Logging.Log(this, Logging.LoggingTypeError, "SQL cmd: " + sqlCmd.ToString(), ex);
					}
					
					conn.Close();
				}
			}
			
			return ret;
		}

		public bool Update(Presentation presentation)
		{
			bool ret = false;
			
			if (presentation == null || presentation.PresentationUID == Guid.Empty || String.IsNullOrEmpty(presentation.Name))
				return false;
			
			using (SqliteConnection conn = GetConnection())
			{
				using (SqliteCommand sqlCmd = new SqliteCommand(@"
					BEGIN; " +
                    "UPDATE presentations SET " +
                    "Name=@Name " +
                    "WHERE PresentationUID=@PresentationUID;" +
				    "COMMIT;", conn))
				{
					sqlCmd.Parameters.AddWithValue("@PresentationUID", presentation.PresentationUID.ToString());
					sqlCmd.Parameters.AddWithValue("@Name", presentation.Name);
					sqlCmd.Parameters.AddWithValue("@DateCreate", DateTime.Now);
					sqlCmd.Parameters.AddWithValue("@Type", (int)presentation.Type);

					conn.Open();
					
					try
					{
						sqlCmd.ExecuteNonQuery();
						ret = true;
					}
					catch (SqliteException ex)
					{
						Logging.Log(this, Logging.LoggingTypeError, "SQL cmd: " + sqlCmd.ToString(), ex);
					}
					
					conn.Close();
				}
			}
			
			return ret;
		}

		public bool Delete(Guid presentationUID)
		{
			bool ret = false;
			
			if (presentationUID == Guid.Empty)
				return false;
			
			using (SqliteConnection conn = GetConnection())
			{
				using (SqliteCommand sqlCmd = new SqliteCommand(@"
					BEGIN; " +
                    "DELETE FROM presentations " +
				    "WHERE PresentationUID=@PresentationUID;" +
					"COMMIT;", conn))
				{
					sqlCmd.Parameters.AddWithValue("@PresentationUID", presentationUID.ToString());

					conn.Open();
					
					try
					{
						sqlCmd.ExecuteNonQuery();
						ret = true;
					}
					catch (SqliteException ex)
					{
						Logging.Log(this, Logging.LoggingTypeError, "SQL cmd: " + sqlCmd.ToString(), ex);
					}
					
					conn.Close();
				}
			}
			
			return ret;
		}
	}
}

