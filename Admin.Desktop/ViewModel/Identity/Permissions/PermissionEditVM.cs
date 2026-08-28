using Admin.Desktop.View.Permissions;
using Admin.Permissions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.PermissionManagement;

namespace Admin.Desktop.ViewModel.Permissions
{
    public partial class PermissionEditVM : ObservableObject, ITransientDependency
    {
        private readonly IPermissionAppService _permissionAppService;
        private readonly ILogger<PermissionEditVM> _logger;
        private string _providerName = string.Empty;
        private string _providerKey = string.Empty;

        [ObservableProperty]
        public partial ObservableCollection<PermissionGroupTreeDto> PermissionGroups { get; set; } = new ObservableCollection<PermissionGroupTreeDto>();

        [ObservableProperty]
        public partial string DialogContainerToken { get; set; } = Guid.NewGuid().ToString();

        public PermissionEditView Owner { get; private set; } = null!;

        public PermissionEditVM(IPermissionAppService permissionAppService, ILogger<PermissionEditVM> logger)
        {
            _permissionAppService = permissionAppService;
            _logger = logger;
        }

        internal async Task InitialAsync(PermissionEditView owner, string providerName, string providerKey)
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                Owner = owner;
                _providerName = providerName;
                _providerKey = providerKey;
                //获取扁平权限分组
                var result = await _permissionAppService.GetAsync(providerName, providerKey);
                var flatGroups = result.Groups;
                // 转换为树形结构
                var treeResult = PermissionTreeConverter.ConvertToTree(flatGroups);
                PermissionGroups = new ObservableCollection<PermissionGroupTreeDto>(treeResult);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            var loadDialog = Dialog.Show<LoadingCircle>(DialogContainerToken);
            try
            {
                var permissionDtos = PermissionTreeConverter.ToFlatPermissionList(PermissionGroups.ToList());
                var permissions = permissionDtos.Select(x => new UpdatePermissionDto
                {
                    Name = x.Name,
                    IsGranted = x.IsGranted,
                }).ToArray();
                await _permissionAppService.UpdateAsync(_providerName, _providerKey, new UpdatePermissionsDto
                {
                    Permissions = permissions
                });
                Owner.DialogResult = true;
                Owner.Close();
            }
            catch (AbpRemoteCallException abpRemoteCallException)
            {
                _logger.LogException(abpRemoteCallException);
                if (!string.IsNullOrEmpty(abpRemoteCallException.Details))
                {
                    MessageBox.Error(abpRemoteCallException.Details, abpRemoteCallException.Message);
                }
                else
                {
                    MessageBox.Error(abpRemoteCallException.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                MessageBox.Error(ex.Message);
            }
            finally
            {
                loadDialog.Close();
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            Owner.Close();
        }
    }
}
