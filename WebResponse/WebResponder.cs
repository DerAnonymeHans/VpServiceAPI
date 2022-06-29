using System;
using System.Threading.Tasks;
using VpServiceAPI.Exceptions;
using VpServiceAPI.Interfaces;

namespace VpServiceAPI.WebResponse
{
    public class WebResponder
    {
        private readonly string ControllerName;
        private readonly LogArea Logarea;
        private readonly bool IsAdmin;
        private readonly IMyLogger Logger;
        public WebResponder(string controllerName, LogArea logArea, bool isAdmin, IMyLogger logger)
        {
            ControllerName = controllerName;
            Logarea = logArea;
            IsAdmin = isAdmin;
            Logger = logger;
        }
        public async Task<WebResponse<T>> RunWith<T>(Func<Task<T>> getBodyFunc, string? action, string? successMsg=null, bool isMessage=true)
        {
            try
            {
                return new WebResponse<T>
                {
                    Body = await getBodyFunc(),
                    IsSuccess = true,
                    Message = successMsg
                };
            }catch(AppException ex)
            {
                return new WebResponse<T>
                {
                    IsSuccess = false,
                    Message = isMessage ? ex.Message : null
                };
            }catch(Exception ex)
            {
                Logger.Error(Logarea, ex, $"Failed to handle {action} on {ControllerName}-Controller.");
                return new WebResponse<T>
                {
                    IsSuccess = false,
                    Message = IsAdmin 
                        ? ex.Message 
                        : isMessage ? "Leider ist ein unerwarteter Fehler aufgetreten. Bitte entschuldige die Unannehmlichkeiten und versuche es später noch einmal." : null
                };
            }
        }

        public WebResponse<T> RunWithSync<T>(Func<T> getBodyFunc, string? action, string? successMsg=null, bool isMessage=true)
        {
            try
            {
                return new WebResponse<T>
                {
                    Body = getBodyFunc(),
                    IsSuccess = true,
                    Message = successMsg
                };
            }
            catch (AppException ex)
            {
                return new WebResponse<T>
                {
                    IsSuccess = false,
                    Message = isMessage ? ex.Message : null
                };
            }
            catch (Exception ex)
            {
                Logger.Error(Logarea, ex, $"Failed to handle {action} on {ControllerName}-Controller.");
                return new WebResponse<T>
                {
                    IsSuccess = false,
                    Message = IsAdmin
                        ? ex.Message
                        : isMessage ? "Leider ist ein unerwarteter Fehler aufgetreten. Bitte entschuldige die Unannehmlichkeiten und versuche es später noch einmal." : null
                };
            }
        }

        public async Task<WebMessage> RunWith(Func<Task> doSomethingFunc, string? action, string? successMsg=null, bool isMessage = true)
        {
            try
            {
                await doSomethingFunc();
                return new WebMessage
                {
                    IsSuccess = true,
                    Message = successMsg
                };
            }
            catch (AppException ex)
            {
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = isMessage ? ex.Message : null
                };
            }
            catch (Exception ex)
            {
                Logger.Error(Logarea, ex, $"Failed to handle {action} on {ControllerName}-Controller.");
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = IsAdmin
                        ? ex.Message
                        : isMessage ? "Leider ist ein unerwarteter Fehler aufgetreten. Bitte entschuldige die Unannehmlichkeiten und versuche es später noch einmal." : null
                };
            }
        }
        public WebMessage RunWithSync(Action doSomethingFunc, string? action, string? successMsg=null, bool isMessage = true)
        {
            try
            {
                doSomethingFunc();
                return new WebMessage
                {
                    IsSuccess = true,
                    Message = successMsg
                };
            }
            catch (AppException ex)
            {
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = isMessage ? ex.Message : null
                };
            }
            catch (Exception ex)
            {
                Logger.Error(Logarea, ex, $"Failed to handle {action} on {ControllerName}-Controller.");
                return new WebMessage
                {
                    IsSuccess = false,
                    Message = IsAdmin
                        ? ex.Message
                        : isMessage ? "Leider ist ein unerwarteter Fehler aufgetreten. Bitte entschuldige die Unannehmlichkeiten und versuche es später noch einmal." : null
                };
            }
        }
    }
}
