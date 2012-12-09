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
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;


namespace De.Dhoffmann.Mono.FullscreenPresentation.Data
{
	public abstract class DataBase
	{
		public DataBase ()
		{
		}
		
		
		protected SqliteConnection GetConnection()
		{
			string docPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dbFilename = Path.Combine(docPath, DBHelper.DATABASENAME);
			
			bool dbExists = File.Exists (dbFilename);
			
			// Existiert die DB schon?
			if (!dbExists)
				SqliteConnection.CreateFile(dbFilename);
			
			SqliteConnection conn = new SqliteConnection("Data Source=" + dbFilename);
			
			// Wenn es eine neue Datenbank ist.. 
			// Eine neue Tabelle für die Versionsverwaltung anlegen.
			if (!dbExists)
			{
				List<SqliteCommand> sqlCmds = new List<SqliteCommand>();

				sqlCmds.Add(new SqliteCommand("BEGIN;", conn));
				sqlCmds.Add(new SqliteCommand("CREATE TABLE version (VersionID INTEGER PRIMARY KEY AUTOINCREMENT, DateCreate DATETIME NOT NULL);", conn));
				sqlCmds.Add(new SqliteCommand("INSERT INTO version (VersionID, DateCreate) VALUES (0, date('now'));", conn));
				sqlCmds.Add(new SqliteCommand("COMMIT;", conn));

				conn.Open();

				foreach(SqliteCommand cmd in sqlCmds)
					cmd.ExecuteNonQuery();
				
				conn.Close();
			}
			
			return conn;
		}
	}
}

