using Bridge.Html5;

namespace MVVM.ViewModels
{
    public class Person
    {
        public Observable<string> FirstName { get; set; }
        public Observable<string> LastName { get; set; }
        public Observable<bool> EditMode { get; set; }
        public Observable<string> FullName { get; set; }
        public Observable<string> FullNameCapitalized { get; set; }
        public Person()
        {
            FullName = Observable.Of(() => FirstName?.Data + " " + LastName?.Data);
            FullNameCapitalized = Observable.Of(() => FullName?.Data?.ToUpperCase());
            EditMode = Observable.Of(false);
        }

        public void Edit()
        {
            EditMode.Data = !EditMode.Data;
        }
    }
}
