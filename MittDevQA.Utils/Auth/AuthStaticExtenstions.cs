using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Auth
{
    public static class AuthStaticExtenstions
    {
        public static IEnumerable<PermissionDTO> GetPermissionsFromEnum(Type enumType)
            => Enum.GetNames(enumType).Select(t => new PermissionDTO
            {
                Name = t,
                Value = (long)Enum.Parse(enumType, t)
            })
                .ToList();

        public static PermissionEnumDTO GetPermissionFromEnum(long enumValue, Type enumType)
        {
            var o = Enum.Parse(enumType, enumValue.ToString());
            var names = Enum.Parse(enumType, enumValue.ToString()).ToString().Split(',');

            return new PermissionEnumDTO
            {
                PermissionDtos = names.Select(t => new PermissionDTO
                {
                    Name = t,
                    Value = (long)Enum.Parse(enumType, t)
                })
                    .ToList(),
                EnumValue = (long)o
            };
        }

        public static List<PermissionGroupDTO> GetPermissionsGroupsFromEnum(Type enumType)
            => GetPermissionsFromEnum(enumType).GroupBy(x => x.Name.Split('_')[0])
                .Select(x => new PermissionGroupDTO { Key = x.Key, Permissions = x.ToList() }).ToList();
    }

    public class PermissionGroupDTO
    {
        public string KeyAR { get; set; }
        public string Key { get; set; }
        public List<PermissionDTO> Permissions { get; set; }
    }

    public class PermissionEnumDTO
    {
        public PermissionEnumDTO()
        {
            PermissionDtos = new List<PermissionDTO>();
        }

        public List<PermissionDTO> PermissionDtos { get; set; }
        public long EnumValue { get; set; }
    }

    public class PermissionDTO
    {
        public string NameAR { get; set; }
        public string Name { get; set; }
        public long Value { get; set; }
    }
}