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
    }
}