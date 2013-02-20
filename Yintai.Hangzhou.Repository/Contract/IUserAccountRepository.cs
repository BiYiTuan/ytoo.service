using System.Collections.Generic;
using Yintai.Hangzhou.Data.Models;
using Yintai.Hangzhou.Model.Enums;

namespace Yintai.Hangzhou.Repository.Contract
{
    public interface IUserAccountRepository : IRepository<UserAccountEntity, int>
    {
        /// <summary>
        /// ��ȡ�û��˻�
        /// </summary>
        /// <param name="userId">userid</param>
        /// <param name="accountType">accountType</param>
        /// <returns></returns>
        UserAccountEntity GetItem(int userId, AccountType accountType);

        /// <summary>
        /// ��ȡ��ǰ�û����˻���Ϣ
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<UserAccountEntity> GetUserAccount(int userId);

        /// <summary>
        /// ��ȡ
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        List<UserAccountEntity> GetListByUserIds(List<int> userIds);

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountType"></param>
        /// <param name="amount"></param>
        void SetAmount(int userId, AccountType accountType, decimal amount);

        /// <summary>
        /// �˻��Ӳ���
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountType"></param>
        /// <param name="amount"></param>
        /// <param name="updateUser"></param>
        /// <returns></returns>
        int AddCount(int userId, AccountType accountType, decimal amount, int? updateUser);

        /// <summary>
        /// �˻�������
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountType"></param>
        /// <param name="amount"></param>
        /// <param name="updateUser"></param>
        /// <returns></returns>
        int SubCount(int userId, AccountType accountType, decimal amount, int? updateUser);
    }
}