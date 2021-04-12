﻿using CoreProject.Helpers;
using CoreProject.Models;
using CoreProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreProject.Managers
{
    public class AccountManager : DBBase
    {
        public void CreateAccount()
        {
        }

        public void UpdateAccount()
        {
        }

        public void DeleteAccount()
        {
        }
        const string connectionString = "Data Source=localhost\\SQLExpress;Initial Catalog=HWDB1; Integrated Security=true";
        public List<AccountModel> GetAccounts()
        {
            string queryString =
                $@" SELECT * FROM Accounts";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    List<AccountModel> list = new List<AccountModel>();

                    while (reader.Read())
                    {
                        AccountModel model = new AccountModel();
                        model.ID = (Guid)reader["ID"];
                        model.Name = (string)reader["Name"];
                        model.Email = (string)reader["Email"];
                        model.Level = (int)reader["Level"];
                        model.PWD = (string)reader["PWD"];

                        list.Add(model);
                    }

                    reader.Close();

                    return list;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        public AccountModel GetAccount(string name)
        {
            string queryString =
                $@" SELECT * FROM Accounts
                    WHERE Name = @name
                ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@name", name);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    AccountModel model = null;

                    while (reader.Read())
                    {
                        model = new AccountModel();
                        model.ID = (Guid)reader["ID"];
                        model.Name = (string)reader["Name"];
                        model.Email = (string)reader["Email"];
                        model.Level = (int)reader["UserLevel"];
                        model.PWD = (string)reader["PWD"];
                    }

                    reader.Close();

                    return model;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        #region ViewModel
        public List<AccountViewModel> GetAccountViewModels(
            string name, int? level,
            out int totalSize, int currentPage = 1, int pageSize = 10)
        {
            //----- Process filter conditions -----
            List<string> conditions = new List<string>();

            if (!string.IsNullOrEmpty(name))
                conditions.Add(" AccountInfos.Name LIKE '%' + @name + '%'");

            if (level.HasValue)
                conditions.Add(" UserLevel = @level");

            string filterConditions =
                (conditions.Count > 0)
                    ? (" WHERE " + string.Join(" AND ", conditions))
                    : string.Empty;
            //----- Process filter conditions -----


            string query =
                $@" 
                    SELECT TOP {10} * FROM
                    (
                        SELECT 
                            ROW_NUMBER() OVER(ORDER BY Accounts.ID) AS RowNumber,
                            Accounts.ID,
                            Accounts.Name AS Account,
                            Accounts.UserLevel,
                            AccountInfos.Name,
                            AccountInfos.Title
                        FROM Accounts
                        JOIN AccountInfos
                        ON Accounts.ID = AccountInfos.ID
                        {filterConditions}
                    ) AS TempT
                    WHERE RowNumber > {pageSize * (1 - 1)}
                    ORDER BY ID
                ";

            string countQuery =
                $@" SELECT 
                        COUNT(Accounts.ID)
                    FROM Accounts
                    JOIN AccountInfos
                    ON Accounts.ID = AccountInfos.ID
                    {filterConditions}
                ";

            List<SqlParameter> dbParameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(name))
                dbParameters.Add(new SqlParameter("@name", name));

            if (level.HasValue)
                dbParameters.Add(new SqlParameter("@level", level.Value));


            var dt = this.GetDataTable(query, dbParameters);

            List<AccountViewModel> list = new List<AccountViewModel>();

            foreach (DataRow dr in dt.Rows)
            {
                AccountViewModel model = new AccountViewModel();
                model.ID = (Guid)dr["ID"];
                model.Name = (string)dr["Name"];
                model.Title = (string)dr["Title"];
                model.Account = (string)dr["Account"];
                model.UserLevel = (int)dr["UserLevel"];

                list.Add(model);
            }


            // 算總數並回傳
            int? totalSize2 = this.GetScale(countQuery, dbParameters) as int?;
            totalSize = (totalSize2.HasValue) ? totalSize2.Value : 0;
            totalSize = 33;

            return list;
        }


        public AccountViewModel GetAccountViewModel(Guid id)
        {
            string queryString =
                $@" SELECT 
                        Accounts.ID,
                        Accounts.Name AS Account,
                        Accounts.UserLevel,
                        Accounts.PWD,
                        Accounts.Email,
                        AccountInfos.Name,
                        AccountInfos.Title,
                        AccountInfos.Phone
                    FROM Accounts
                    JOIN AccountInfos
                        ON Accounts.ID = AccountInfos.ID
                    WHERE Accounts.ID = @id
                ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    AccountViewModel model = null;

                    while (reader.Read())
                    {
                        model = new AccountViewModel();
                        model.ID = (Guid)reader["ID"];
                        model.Name = (string)reader["Name"];
                        model.Title = (string)reader["Title"];
                        model.Account = (string)reader["Account"];
                        model.UserLevel = (int)reader["UserLevel"];
                        model.PWD = (string)reader["PWD"];
                        model.Email = (string)reader["Email"];
                        model.Phone = (string)reader["Phone"];
                    }

                    reader.Close();

                    return model;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public void CreateAccountViewModel(AccountViewModel model)
        {
            // Check account is repeated.
            if (this.GetAccount(model.Account) != null)
            {
                throw new Exception($"Account [{model.Account}] has been created.");
            }


            string dbCommandText =
                $@" INSERT INTO Accounts 
                        (ID, NAME, PWD,UserLevel,Email) 
                    VALUES 
                        (@id, @account, @PWD, @UserLevel, @Email);

                    INSERT INTO AccountInfos 
                        (ID, NAME, PHONE, TITLE) 
                    VALUES 
                        (@id, @name, @PHONE, @Title);
                ";

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@id", Guid.NewGuid()),
                new SqlParameter("@account", model.Account),
                new SqlParameter("@PWD", model.PWD),
                new SqlParameter("@UserLevel", model.UserLevel),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@name", model.Name),
                new SqlParameter("@PHONE", model.Phone),
                new SqlParameter("@Title", model.Title)
            };

            this.ExecuteNonQuery(dbCommandText, parameters);
        }

        public void UpdateAccountViewModel(AccountViewModel model)
        {
            string dbCommandText =
                $@" UPDATE Accounts 
                        SET PWD = @PWD, UserLevel = @UserLevel, Email = @Email  
                    WHERE ID = @id;

                    UPDATE AccountInfos 
                        SET NAME = @name, PHONE = @Email, TITLE = @Email 
                    WHERE ID = @id;
                ";

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@id", model.ID),
                new SqlParameter("@PWD", model.PWD),
                new SqlParameter("@UserLevel", model.UserLevel),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@name", model.Name),
                new SqlParameter("@PHONE", model.Phone),
                new SqlParameter("@Title", model.Title)
            };

            this.ExecuteNonQuery(dbCommandText, parameters);
        }

        public void DeleteAccountViewModel(Guid id)
        {
            string dbCommandText =
                $@" DELETE AccountInfos WHERE ID = @id;
                    DELETE Accounts WHERE ID = @id;
                ";

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@id", id),
            };

            this.ExecuteNonQuery(dbCommandText, parameters);
        }
        #endregion
    }
}
