using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface ICategoryEditService
    {
        /// <summary>
        /// The load common categroies.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<Categories> LoadCommonCategories(string ic);

        /// <summary>
        /// The load local categroies.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<Categories> LoadLocalCategories(string ic);

        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="category_name">
        /// The category_name.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string run_db(string act, int category_id, string category_name, string ic, string userid);
    }
}