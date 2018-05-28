using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/**
 * Experimental nuclei half lives for known nuclei according to Nubase 2016 [1].
 * In the case where halflife is not experimentally known, its partial halflives are calculated
 *      according to the formulas by Karpov et al. in [2] using ground state mass excesses provided
 *      by Möller et al. in [3] and calculated fission barriers provided by Möller et al. in [4].
 * The partial halflives are ONLY calculated for nuclei satisfying the following criteria:
 *  * Z geq 82 (lead)
 *  * N geq 126 (e.g. lead-208, last magic number with a stable nuclide)
 *  * Ground state masses for the nuclide, its potential alpha daughter (Z-2, N-2), its potential
 *      β- daughter (Z+1, N-1), and its potential β+ daughter (Z-1, N+1) are all given in [3]
 *  * N leq 1.55*Z + 22 (this line runs parallel to Uranium's neutron ratio N = 1.55Z, and runs 
 *      through the lowest-A point on a "gulf of instability", a region at which predictions become
 *      highly model dependent. All nuclei past this line have T_1/2 less than 10 seconds in this model.
 * Nuclei with no experimentally determined halflife that fail one of the above criteria are treated
 * as if they have a halflife of zero, as are nuclei with experimental halflives less than 1 μs.
 * 
 * Relative abundances from AGW Cameron [5], converted to ppm / ppt. Technetium, Promethium, and actinides
 * up to curium occur naturally in uranium ores at concentrations less than 5 ppm relative to the ore, so they are
 * ignored even though they occur in detectable quantities.
 * 
 * 
 * [1]: Audi, G. et al., "The NUBASE2016 evaluation of nuclear properties"
 *      DOI: 10.1088/1674-1137/41/3/030001
 *      data at https://www-nds.iaea.org/amdc/ame2016/nubase2016.txt
 * [2]: Karpov, A. et al., "Decay properties and stability of heaviest elements"
 *      DOI: 10.1142/S0218301312500139  http://nrv.jinr.ru/karpov/publications/Karpov12_IJMPE.pdf
 * [3]: Möller, P. et al., "Nuclear ground-state masses and deformations: FRDM(2012)"
 *      doi:10.1016/j.adt.2015.10.002
 *      data at https://www-nds.iaea.org/ripl/masses/recommended/moller.gz 
 *      readme at https://www-nds.iaea.org/ripl/masses/recommended/moller_readme.htm
 * [4]: Möller, P. et al., "Fission barriers at the end of the chart of the nuclides"
 *      doi:10.1103/PhysRevC.91.024310
 *      data at http://t2.lanl.gov/nis/molleretal/publications/onebar-forprc.dat
 * [5]: Cameron, AGW, "Abundances of the Elements in the Solar System"
 *      doi:10.1007/BF00172440 https://pubs.giss.nasa.gov/docs/1973/1973_Cameron_ca06310p.pdf
 **/

public class Constants : MonoBehaviour {
    public static Texture2D halflifemap;
    public static double[,][,] decaytypes;
    public static float scaleFactor = 0.01f;
    public static float EM_CONSTANT = 0.2f;
    public static float SF_CONSTANT = 0.1f;
    public static int MAXP = 119;
    public static int MAXN = 230;
    public static readonly float electronMass_amu = 5.486e-4f;
    public static readonly string electronDiscoveryYear = "1897";
    public static readonly string positronDiscoveryYear = "1932";

    public static readonly string[] photoCredits = new string[] {
        "Steve Bowers", "NASA", "PeriodicTable.ru", //He
        "Wikipedia (Public Domain)", "Guanaco & Alchemist-hp (CC)", "PeriodicTable.ru", //B
        "Robert Lavinsky (CC)", "PeriodicTable.ru", "PeriodicTable.ru", "Teach Nuclear", "PeriodicTable.ru", //Ne
        "Dnn87 (CC)", "Warut Roonguthai (CC)", "Wikipedia (CC)", "PeriodicTable.ru", //Si
        "Peter Krimbacher (CC)", "Wikipedia (Public Domain)", "W. Oelen (CC)", "PeriodicTable.ru", //Ar
        "Dnn87 (CC)", "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", //V
        "PeriodicTable.ru", "Tomihahndorf (CC)", "PeriodicTable.ru", "PeriodicTable.ru", //Co
        "images-of-elements", "Digon3 & Materialscientist (CC)", "PeriodicTable.ru", "foobar (CC)", //Ga
        "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", "images-of-elements", "PeriodicTable.ru", //Kr
        "Dnn87 (CC)", "PeriodicTable.ru", "PeriodicTable.ru", "images-of-elements", "PeriodicTable.ru", //Nb
        "images-of-elements", "images-of-elements (Public Domain)", "PeriodicTable.ru", "PeriodicTable.ru", //Rh
        "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", "Wikipedia (Public Domain)", //In
        "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", "PeriodicTable.ru", //Xe
        "PeriodicTable.ru", "images-of-elements", "images-of-elements", "images-of-elements", //Pr
        "images-of-elements", "images-of-elements", "images-of-elements (artist's impression)", "images-of-elements", "images-of-elements", "images-of-elements", //Gd
        "images-of-elements", "images-of-elements", "images-of-elements", "images-of-elements", //Er
        "PeriodicTable.ru", "images-of-elements", "images-of-elements", "images-of-elements", //Hf
        "images-of-elements", "PeriodicTable.ru", "images-of-elements", "PeriodicTable.ru", //Os
        "images-of-elements", "PeriodicTable.ru", "PeriodicTable.ru", "Bionerd (CC)", //Hg
        "W. Oelen (CC)", "images-of-elements", "PeriodicTable.ru", //Bi
        "images-of-elements (artist's impression)", "images-of-elements (artist's impression)", "images-of-elements (artist's impression)", //Rn
        "images-of-elements (artist's impression)", "grenadier (CC)", "Ralph E. Lapp (LIFE 1965)", //Ac
        "PeriodicTable.ru", "EU JRC-ITU", "USDOE (Public Domain)", "D. Aoki https://doi.org/10.1143/JPSJ.73.519", //Np
        "LANL (Public Domain)", "Bionerd (CC)", "A. Kronenberg", "USDOE (Public Domain)", "USDOE (Public Domain)", //Es
        "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", //Mt
        "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?" //Uue
    };

    public static readonly string[] elementNames = new string[] {
        "Neutron", "Hydrogen", "Helium", "Lithium", "Beryllium", "Boron", "Carbon", "Nitrogen", "Oxygen", "Fluorine", "Neon",
        "Sodium", "Magnesium", "Aluminium", "Silicon", "Phosphorus", "Sulfur", "Chlorine", "Argon",
        "Potassium", "Calcium", "Scandium", "Titanium", "Vanadium", "Chromium", "Manganese", "Iron", "Cobalt",
        "Nickel", "Copper", "Zinc", "Gallium", "Germanium", "Arsenic", "Selenium", "Bromine", "Krypton",
        "Rubidium", "Strontium", "Yttrium", "Zirconium", "Niobium", "Molybdenum", "Technetium", "Ruthenium", "Rhodium",
        "Palladium", "Silver", "Cadmium", "Indium", "Tin", "Antimony", "Tellurium", "Iodine", "Xenon",
        "Caesium", "Barium", "Lanthanum", "Cerium", "Praseodymium", "Neodymium", "Promethium", "Samarium", "Europium",
        "Gadolinium", "Terbium", "Dysprosium", "Holmium", "Erbium", "Thulium", "Ytterbium", "Lutetium",
        "Hafnium", "Tantalum", "Tungsten", "Rhenium", "Osmium", "Iridium",
        "Platinum", "Gold", "Mercury", "Thallium", "Lead", "Bismuth", "Polonium", "Astatine", "Radon",
        "Francium", "Radium", "Actinium", "Thorium", "Protactinium", "Uranium", "Neptunium", "Plutonium", "Americium",
        "Curium", "Berkelium", "Californium", "Einsteinium", "Fermium", "Mendelevium", "Nobelium", "Lawrencium",
        "Rutherfordium", "Dubnium", "Seaborgium", "Bohrium", "Hassium", "Meitnerium",
        "Darmstadtium", "Roentgenium", "Copernicium", "Nihonium", "Flerovium", "Moscovium", "Livermorium", "Tennessine", "Oganesson",
        "Ununennium (Placeholder)"
    };

    public static readonly string[] discoveryYears = new string[] {
        "1932", "1766", "1868", //He
        "1817", "1798", "1808", "~3700 BC", "1772", "1771", "1810", "1898", //Ne
        "1807", "1755", "1825", "1823", "1669", "~2500 BC", "1774", "1894", //Ar
        "1807", "1808", "1879", "1791", "1801", "1797", "1774", "~6000 BC", "1735", "1751", "~9000 BC", "~1500 BC", //Zn
        "1875", "1886", "~ 800", "1817", "1825", "1898", //Kr
        "1861", "1787", "1794", "1789", "1801", "1778", "1937", "1844", "1804", "1803", "~5000 BC", "1817", //Cd
        "1863", "~3500 BC", "~ 800", "1782", "1811", "1898", //Xe
        "1860", "1772", "1838", //La
        "1803", "1882", "1882", "1942", "1879", "1896", "1880", "1842", "1886", "1878", "1842", "1879", "1878", "1906", //Lu
        "1922", "1802", "1781", "1908", "1803", "1803", "1748", "~6500 BC", "~2500 BC", //Hg
        "1861", "~7000 BC", "1753", "1898", "1940", "1899", //Rn
        "1939", "1898", "1902", //Ac
        "1829", "1913", "1789", "1940", "1940", "1944", "1944", "1949", "1950", "1952", "1952", "1955", "1966", "1961", //Lr
        "1969", "1970", "1974", "1981", "1984", "1982", "1995", "1995", "1996", "2004", "2004", "2010", "2004", "2010", "2006", //Og
        "Unconfirmed", "Unconfirmed", "Unconfirmed", "Unconfirmed", //Ubb

    };


    public static readonly string[] altElementNames = new String[] {
        "Neutronium", "", "", "", "", "", "", "", "", "", "", //Ne
        "(Natrium)", "", "Aluminum", "", "", "Sulphur", "", "", //Ar
        "(Kalium)", "", "", "", "", "", "", "(Ferrum)", "", "", "(Cuprum)", "", "", "", "", "", "", "", //Kr
        "", "", "", "", "Columbium", "", "", "", "", "", "(Argentum)", "", "", "(Stannum)", "(Stibium)", "", "", "", //Xe
        "Cesium", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", //Lu
        "", "", "Wolfram", "", "", "", "", "(Aurum)", "(Hydrargyrum)", "", "(Plumbum)", "", "", "", "", //Rn
        "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", //Lr
        "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "(119)" //Uue
    };

    public static readonly double[] abundances = new double[] {
        0, 0.934, 0.0649, //He
        1.45e-09, 2.38e-11, 1.03e-08, 3.47e-04, 1.1e-04, 6.31e-04, 7.19e-08, 1.01e-04, //Ne
        1.76e-06, 3.12e-05, 2.5e-06, 2.94e-05, 2.82e-07, 1.47e-05, 1.67e-07, 3.44e-06, //Ar
        1.23e-07, 2.12e-06, 1.03e-09, 8.15e-08, 7.69e-09, 3.73e-07, 2.73e-07, 2.44e-05, 6.49e-08, 1.41e-06, 1.59e-08, 3.65e-08, //Zn
        1.41e-09, 3.38e-09, 1.94e-10, 1.97e-09, 3.96e-10, 1.37e-09, //Kr
        1.73e-10, 7.9e-10, 1.41e-10, 8.22e-10, 4.11e-11, 1.17e-10, 0.00, 5.58e-11, 1.17e-11, 3.82e-11, 1.32e-11, 4.35e-11, //Cd
        5.55e-12, 1.06e-10, 9.28e-12, 1.89e-10, 3.2e-11, 1.58e-10, //Xe
        1.14e-11, 1.41e-10, 1.31e-11, //La
        3.47e-11, 4.38e-12, 2.29e-11, 0.00, 6.64e-12, 2.5e-12, 8.72e-12, 1.62e-12, 1.06e-11, 2.32e-12, 6.61e-12, 9.98e-13, 6.34e-12, //Lu
        1.06e-12, 6.17e-12, 6.17e-13, 4.7e-12, 1.56e-12, 2.2e-11, 2.11e-11, 4.11e-11, 5.93e-12, 1.17e-11, //Hg
        5.64e-12, 1.17e-10, 4.2e-12, 0, 0, 0, //Rn
        0, 0, 0, //Ac
        9.98e-13, 0, 7.69e-13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //Lr
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //Cn
        0, 0, 0, 0, 0, 0, //Og
        0
    };

    public static Color AbundanceColor(int Z)
    {
        double abundance = abundances[Z];
        if (abundance == 0)
        {
            abundance = 1e-18;
        }
        float LA = (float)(1/(-0.1*Math.Log(abundance) + 1));
        return new Color(LA, LA, LA);
    }


    public static readonly double[] atomicWeights = new double[] {
        1.0087, 1.0078, 4.0026, //He
        6.94, 9.0122, 10.81, 12.011, 14.007, 15.999, 18.998, 20.180, //Ne
        22.99, 24.305, 26.982, 28.085, 30.974, 32.06, 35.45, 39.948, //Ar
        39.098, 40.078, 44.956, 47.867, 50.942, 51.996, 54.938, 55.845, 58.933, 58.693, 63.546, 65.38, //Zn
        69.723, 72.63, 74.92, 78.971, 79.904, 83.798, //Kr
        85.468, 87.62, 88.906, 91.224, 92.906, 95.95, 98.906, 101.07, 102.91, 106.42, 107.87, 112.41, //Cd
        114.82, 118.71, 121.76, 127.6, 126.90, 131.29, //Xe
        132.91, 137.33, 138.91, //La
        140.12, 140.91, 144.24, 146.915, 150.36, 151.96, 157.25, 158.93, 162.50, 164.93, 167.26, 168.93, 173.05, 174.97, //Lu
        178.49, 180.95, 183.84, 186.21, 190.23, 192.22, 195.08, 196.97, 200.59, //Hg
        204.38, 207.2, 208.98, 209.983, 219.01, 222.02, //Rn
        223.020, 226.03, 227.027, //Ac
        232.04, 231.04, 238.03, 237.048, 239.05, 242, 246, 249, 249, 252, 257, 258, 259, 266, //Lr
        268, 268, 271, 273, 283, 284, 284, 289, 291, 291, 293, 294, 295, 294, 295, //Og
        298, 300
    };

    public static readonly string[] shellTexts = new String[] {
        "[n]", "1s1", "1s2", //He
        "[He] 2s1", "[He] 2s2", "[He] 2s2 2p1", "[He] 2s2 2p2", "[He] 2s2 2p3", "[He] 2s2 2p4", "[He] 2s2 2p5", "[He] 2s2 2p6", //Ne
        "[Ne] 3s1", "[Ne] 3s2", "[Ne] 3s2 3p1", "[Ne] 3s2 3p2", "[Ne] 3s2 3p3", "[Ne] 3s2 3p4", "[Ne] 3s2 3p5", "[Ne] 3s2 3p6", //Ar
        "[Ar] 4s1", "[Ar] 4s2", //Ca
        "[Ar] 3d1 4s2", "[Ar] 3d2 4s2", "[Ar] 3d3 4s2", "[Ar] 3d5 4s1", "[Ar] 3d5 4s2", //Mn
        "[Ar] 3d6 4s2", "[Ar] 3d7 4s2", "[Ar] 3d8 4s2", "[Ar] 3d10 4s1", "[Ar] 3d10 4s2", //Zn
        "[Ar] 3d10 4s2 4p1", "[Ar] 3d10 4s2 4p2", "[Ar] 3d10 4s2 4p3", "[Ar] 3d10 4s2 4p4", "[Ar] 3d10 4s2 4p5", "[Ar]3d10 4s2 4p6", //Kr
        "[Kr] 5s1", "[Kr] 5s2", //Sr
        "[Kr] 4d1 5s2", "[Kr] 4d2 5s2", "[Kr] 4d4 5s1", "[Kr] 4d5 5s1", "[Kr] 4d5 5s2", //Tc
        "[Kr] 4d7 5s1", "[Kr] 4d8 5s1", "[Kr] 4d10", "[Kr] 4d10 5s1", "[Kr] 4d10 5s2", //Cd
        "[Kr] 4d10 5s2 5p1", "[Kr] 4d10 5s2 5p2", "[Kr] 4d10 5s2 5p3", "[Kr] 4d10 5s2 5p4", "[Kr] 4d10 5s2 5p5", "[Kr] 4d10 5s2 5p6", //Xe
        "[Xe] 6s1", "[Xe] 6s2", //Ba
        "[Xe] 5d1 6s2", "[Xe] 4f1 5d1 6s2", "[Xe] 4f3 6s2", "[Xe] 4f4 6s2", "[Xe] 4f5 6s2", //Pm
        "[Xe] 4f6 6s2", "[Xe] 4f7 6s2", "[Xe] 4f7 5d1 6s2", "[Xe] 4f9 6s2", "[Xe] 4f10 6s2", //Dy
        "[Xe] 4f11 6s2", "[Xe] 4f12 6s2", "[Xe] 4f13 6s2", "[Xe] 4f14 6s2", //Yb
        "[Xe] 4f14 5d1 6s2", "[Xe] 4f14 5d2 6s2", "[Xe] 4f14 5d3 6s2", "[Xe] 4f14 5d4 6s2", "[Xe] 4f14 5d5 6s2", //Re
        "[Xe] 4f14 5d6 6s2", "[Xe] 4f14 5d7 6s2", "[Xe] 4f14 5d9 6s1", "[Xe] 4f14 5d10 6s1", "[Xe] 4f14 5d10 6s2", //Hg
        "[Xe] 4f14 5d10 6s2 6p1", "[Xe] 4f14 5d10 6s2 6p2", "[Xe] 4f14 5d10 6s2 6p3", "[Xe] 4f14 5d10 6s2 6p4", "[Xe] 4f14 5d10 6s2 6p5", "[Xe] 4f14 5d10 6s2 6p6",
        "[Rn] 7s1", "[Rn] 7s2", //Ra
        "[Rn] 6d1 7s2", "[Rn] 6d2 7s2", "[Rn] 5f2 6d1 7s2", "[Rn] 5f3 6d1 7s2", "[Rn] 5f4 6d1 7s2", //Np
        "[Rn] 5f6 7s2", "[Rn] 5f7 7s2", "[Rn] 5f7 6d1 7s2", "[Rn] 5f9 7s2", "[Rn] 5f10 7s2", //Bk
        "[Rn] 5f11 7s2", "[Rn] 5f12 7s2", "[Rn] 5f13 7s2", "[Rn] 5f14 7s2", //No
        "[Rn] 5f14 7s2 7p1", "[Rn] 5f14 6d2 7s2", "[Rn] 5f14 6d3 7s2", "[Rn] 5f14 6d4 7s2", "[Rn] 5f14 6d5 7s2", //Bh
        "[Rn] 5f14 6d6 7s2", "<i>[Rn] 5f14 6d7 7s2 (predicted)</i>", "<i>[Rn] 5f14 6d8 7s2 (predicted)</i>", "<i>[Rn] 5f14 6d9 7s2 (predicted)</i>", "[Rn] 5f14 6d10 7s2", //Cn
        "<i> [Rn] 5f14 6d10 7s2 7p1 (predicted)</i>", "<i> [Rn] 5f14 6d10 7s2 7p2 (predicted)</i>", "<i> [Rn] 5f14 6d10 7s2 7p3 (predicted)</i>", //Mc
        "<i> [Rn] 5f14 6d10 7s2 7p4 (predicted)</i>", "<i> [Rn] 5f14 6d10 7s2 7p5 (predicted)</i>", "<i> [Rn] 5f14 6d10 7s2 7p6 (predicted)</i>",
        "<i> [Og] 8s1 (predicted)</i>"
    };

    public static readonly int[] valenceElectrons = new int[] {
    0,
    1,                                                                                           2,
    1, 2,                                                                         3, 4, 5, 6, 7, 8,
    1, 2,                                                                         3, 4, 5, 6, 7, 8,
    1, 2,                                           3, 4, 5, 3, 6, 3, 2, 2, 2, 2, 3, 4, 5, 6, 7, 8,
    1, 2,                                           3, 4, 5, 4, 8, 4, 3, 2, 1, 2, 3, 4, 5, 6, 7, 8,
    1, 2, 3, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 5, 6, 7, 8, 4, 4, 1, 2, 1, 2, 3, 4, 7, 8,
    1, 2, 3, 4, 5, 6, 5, 4, 3, 3, 3, 3, 3, 3, 3, 2, 3, 4, 5, 6, 7, 8, 3, 0, 3, 4, 1, 2, 1, 2, 7, 8,
    1, 2
    };

    public static readonly int[] elementType = new int[] {
    1,
    0,                                                            1,
    3,4,                                                5,0,0,0,0,1,
    3,4,                                                2,5,0,0,0,1,
    3,4,                            2,2,2,2,2,2,2,2,2,2,2,5,5,0,0,1,
    3,4,                            2,2,2,2,2,2,2,2,2,2,2,2,5,5,0,1,
    3,4,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,5,1,
    3,4,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
    3,4
    };

    public static readonly string[] typeNames = new string[] {
        "Nonmetal", "Noble Gas", "Metal", "Alkali", "Alkaline", "Metalloid"
    };

    public static readonly int[] elementType2 = new int[] {
    10,
    0,                                                            1,
    3,4,                                                5,0,0,0,6,1,
    3,4,                                                9,5,0,0,6,1,
    3,4,                            2,2,2,2,2,2,2,2,2,9,9,5,5,0,6,1,
    3,4,                            2,2,2,2,2,2,2,2,2,9,9,9,5,5,6,1,
    3,4,2,7,7,7,7,7,7,7,7,7,7,7,7,7,2,2,2,2,2,2,2,2,2,9,9,9,9,9,5,1,
    3,4,2,8,8,8,8,8,8,8,8,8,8,8,8,8,2,2,2,2,2,2,2,2,2,9,9,9,9,9,9,9,
    3,4
    };

    public static readonly string[] typeNames2 = new string[] {
        "Nonmetal", "Noble Gas", "Transition \n Metal", "Alkali", "Alkaline", "Metalloid", "Halogen", "Lanthanide", "Actinide", "Post-\nTransition \nMetal",
        "Quarks"
    };

    public static readonly Color[] typeColors = new Color[] {
        new Color(1f, 1f, 0f),
        new Color(0f, 1f, 1f),
        new Color(1f, 0.5f, 0.7f),
        new Color(1f, 0.4f, 0.4f),
        new Color(1f, 0.5f, 0.1f),
        new Color(0.5f, 0.7f, 0.3f),
        new Color(0f, 0.4f, 0.1f),
        new Color(1f, 0.5f, 1f),
        new Color(0.8f, 0.3f, 0.8f),
        new Color(0.5f, 0.5f, 0.5f),
        new Color(1f, 1f, 1f)
    };

    public static Color TemperatureToRGB(double T)
    {
        double temp = T / 100;
        double red, green, blue;

        if (temp <= 66)
        {
            red = Math.Max(100*Math.Log(temp),100);
            green = temp;
            green = 99.4708 * Math.Log(green) - 161.1195;
            if (temp <= 19)
            {
                blue = 0;
            }
            else
            {
                blue = temp - 10;
                blue = 138.5177 * Math.Log(blue) - 305.0448;
            }
        }
        else
        {
            red = temp - 60;
            red = 329.698 * Math.Pow(red, -0.1332);

            green = temp - 60;
            green = 288.122 * Math.Pow(green, -0.0755);

            blue = 255;
        }

        red = red / 255;
        green = green / 255;
        blue = blue / 255;

        return new Color(Mathf.Clamp((float)red, 0, 1), Mathf.Clamp((float)green, 0, 1), Mathf.Clamp((float)blue, 0, 1));
    }



    public static readonly string[] subshells = new string[] {
        "1s",
        "1s", "1s", //He
        "2s", "2s", "2p", "2p", "2p", "2p", "2p", "2p", //Ne
        "3s", "3s", "3p", "3p", "3p", "3p", "3p", "3p", //Ar
        "4s", "4s", "3d", "3d", "3d", "3d", "3d", "3d", "3d", "3d", "3d", "3d", "4p", "4p", "4p", "4p", "4p", "4p", //Kr
        "5s", "5s", "4d", "4d", "4d", "4d", "4d", "4d", "4d", "4d", "4d", "4d", "5p", "5p", "5p", "5p", "5p", "5p", //Xe
        "6s", "6s", "5d", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", "4f", //Yb
        "5d", "5d", "5d", "5d", "5d", "5d", "5d", "5d", "5d", "5d", "6p", "6p", "6p", "6p", "6p", "6p", //Rn
        "7s", "7s", "6d", "6d", "5f", "5f", "5f", "5f", "5f", "5f", "5f", "5f", "5f", "5f", "5f", "5f", //No
        "7p", "6d", "6d", "6d", "6d", "6d", "6d", "6d", "6d", "6d", "7p", "7p", "7p", "7p", "7p", "7p", //Og
        "8s", "8s", "8p", "7d", "6f", "6f", "5g", "5g", "5g", "5g", //128 (Ubo)
    };

    public static readonly int[] period = new int[] {
        1, 1, 1, //He
        2, 2, 2, 2, 2, 2, 2, 2, //Ne
        3, 3, 3, 3, 3, 3, 3, 3, //Ar
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, //Kr
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //Xe
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, //Rn
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, //Og
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8 //128 (Ubo
    };



    public static readonly double[] electroNegativities = new double[] {
      0, 2.2, 0, 0.98, 1.57, 2.04, 2.55, 3.04, 3.44, 3.98, 0, //Ne
      0.93, 1.31, 1.61, 1.9, 2.19, 2.58, 3.16, 0, //Ar
      0.82, 1, 1.36, 1.54, 1.63, 1.66, 1.55, 1.83, 1.88, 1.91, 1.9, 1.65, 1.81, 2.01, 2.18, 2.55, 2.96, 3, //Kr
      0.82, 0.95, 1.22, 1.33, 1.6, 2.16, 1.9, 2.2, 2.28, 2.2, 1.93, 1.69, 1.78, 1.96, 2.05, 2.1, 2.66, 2.6, //Xe
      0.79, 0.89, 1.1, 1.12, 1.13, 1.14, 1.13, 1.17, 1.2, 1.2, 1.1, 1.22, 1.23, 1.24, 1.25, 1.1, 1.27, //Lu
      1.3, 1.5, 2.36, 1.9, 2.2, 2.2, 2.28, 2.54, 2, 1.62, 1.87, 2.02, 2, 2.2, 2.2, //Rn
      0.7, 0.9, 1.1, 1.3, 1.5, 1.38, 1.36, 1.28, 1.13, 1.28, 1.3, 1.3, 1.3, 1.3, 1.3, 1.3, 1.3, //Lr
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //118
      0, 0 //Ubn
    };

    public static readonly double[] ionizationEnergies = new double[] {
        0, 13.598, 24.59, //He
        5.392, 9.322, 8.298, 11.260, 14.534, 13.62, 17.42, 21.56, //Ne
        5.139, 7.646, 5.986, 8.152, 10.487, 10.360, 12.968, 15.760, //Ar
        4.341, 6.113, 6.562, 6.828, 6.746, 6.767, 7.434, 7.902, 7.881, 7.640, 7.726, 9.394, //Zn
        5.999, 7.899, 9.789, 9.752, 11.814, 14.000, //Kr
        4.177, 5.695, 6.217, 6.634, 6.759, 7.092, 7.28, 7.361, 7.459, 8.337, 7.576, 8.994, //Cd
        5.786, 7.344, 8.608, 9.010, 10.45, 12.13, //Xe
        3.894, 5.212, 5.577, //La
        5.539, 5.473, 5.525, 5.582, 5.644, 5.670, 6.150, 5.864, 5.939, 6.022, 6.108, 6.184, 6.254, 5.426, //Lu
        6.825, 7.550, 7.864, 7.834, 8.438, 8.967, 8.959, 9.226, 10.44, //Hg
        6.108, 7.417, 7.286, 8.417, 9.318, 10.748, //Rn
        4.072, 5.278, 5.17, //Ac
        6.307, 5.89, 6.194, 6.266, 6.026, 5.974, 5.992, 6.198, 6.282, 6.42, 6.50, 6.58, 6.65, 4.96, //Lr
        6.0 //Rf, no further empirical data
    };

    public static readonly int[][] oxidationStates = new int[][] {
        new int[] {0,0,0,0,2,0,0,0,0,0,0,0,0},      new int[] {0,0,0,2,1,1,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,2,0,0,0,0,0,0,0,0},      new int[] {0,0,0,0,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {2,1,1,1,1,1,1,1,1,0,0,0,0},      new int[] {0,2,1,1,1,1,1,1,1,1,0,0,0},
        new int[] {0,0,2,1,1,1,1,0,0,0,0,0,0},      new int[] {0,0,0,2,1,0,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,2,0,0,0,0,0,0,0,0},      new int[] {0,0,0,1,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {2,1,1,1,1,1,1,1,1,0,0,0,0},      new int[] {0,2,1,1,1,1,1,1,1,1,0,0,0},
        new int[] {0,0,2,1,1,1,1,1,1,1,1,0,0},      new int[] {0,0,0,2,1,1,1,1,1,1,1,1,0},
        new int[] {0,0,0,0,2,0,0,0,0,0,0,0,0},      new int[] {0,0,0,1,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,1,1,1,1,1,2,0,0,0,0},      new int[] {0,0,0,1,1,1,1,1,1,2,0,0,0},
        new int[] {0,0,1,1,1,1,1,2,1,1,1,0,0},      new int[] {0,1,1,1,1,1,2,1,1,1,1,1,0},
        new int[] {0,0,1,1,1,1,1,2,1,1,1,0,0},      new int[] {0,0,0,1,1,1,2,1,1,1,0,0,0},
        new int[] {0,0,0,1,1,1,2,1,1,0,0,0,0},      new int[] {0,0,0,0,1,1,2,1,1,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {2,1,1,1,1,1,1,1,1,0,0,0,0},      new int[] {0,2,0,0,1,1,1,1,0,1,0,0,0},
        new int[] {0,0,2,0,1,1,1,0,1,0,1,0,0},      new int[] {0,0,0,2,1,1,1,1,1,1,0,1,0},
        new int[] {0,0,0,0,2,0,1,0,0,0,0,0,0},      new int[] {0,0,0,1,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,1,1,2,0,0,0,0},      new int[] {0,0,0,1,1,1,1,1,1,2,0,0,0},
        new int[] {0,0,1,1,1,1,1,1,2,1,1,0,0},      new int[] {0,1,0,1,1,1,1,1,2,1,1,1,0},
        new int[] {0,0,1,0,1,1,1,1,2,1,1,1,1},      new int[] {0,0,0,1,1,1,1,2,1,1,1,0,0},
        new int[] {0,0,0,0,1,1,2,0,1,0,1,0,0},      new int[] {0,0,0,0,1,2,1,1,1,0,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},
        new int[] {1,0,0,0,1,0,1,0,2,0,0,0,0},      new int[] {0,2,0,0,1,0,0,1,0,1,0,0,0},
        new int[] {0,0,2,0,1,0,1,0,1,1,1,0,0},      new int[] {0,0,0,2,1,1,0,1,1,1,0,1,0},
        new int[] {0,0,0,0,2,1,1,0,1,0,1,0,1},      new int[] {0,0,0,1,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,1,2,0,0,0,0},      new int[] {0,0,0,0,1,0,1,1,2,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,1,1,2,0,0,0,0,0},      new int[] {0,0,0,0,1,1,1,2,1,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},      new int[] {0,0,0,0,1,0,0,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,1,2,0,0,0,0},      new int[] {0,0,0,1,1,0,1,1,1,2,0,0,0},
        new int[] {0,0,1,1,1,1,1,1,1,1,2,0,0},      new int[] {0,1,0,1,1,1,1,1,1,1,1,2,0},
        new int[] {0,0,1,1,1,1,1,1,1,1,1,1,2},      new int[] {0,1,0,1,1,1,1,1,2,1,1,1,1},
        new int[] {0,0,1,1,1,1,1,1,2,1,1,0,0},      new int[] {0,0,0,1,1,2,1,1,0,1,0,0,0},
        new int[] {0,0,0,0,1,1,2,0,1,0,0,0,0},      new int[] {0,0,0,1,1,2,0,1,0,0,0,0,0},
        new int[] {1,0,0,0,1,0,2,0,1,0,0,0,0},      new int[] {0,1,0,0,1,1,0,2,0,1,0,0,0},
        new int[] {0,0,1,0,1,0,1,0,2,1,1,0,0},      new int[] {0,0,0,2,1,1,0,1,0,1,0,1,0},
        new int[] {0,0,0,0,2,0,1,0,0,0,1,0,0},      new int[] {0,0,0,0,1,2,0,0,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,2,0,0,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,1,2,0,0,0,0},      new int[] {0,0,0,0,1,0,1,1,1,2,0,0,0},
        new int[] {0,0,0,0,1,0,1,1,1,1,2,0,0},      new int[] {0,0,0,0,1,0,0,1,1,2,1,1,0},
        new int[] {0,0,0,0,1,0,0,1,2,1,1,1,1},      new int[] {0,0,0,0,1,0,1,2,1,1,1,1,0},
        new int[] {0,0,0,0,1,0,1,2,1,0,1,0,1},      new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,1,0,0,0,0},
        new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},      new int[] {0,0,0,0,1,0,1,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,2,1,0,0,0,0,0},      new int[] {0,0,0,0,1,0,0,2,0,0,0,0,0},
        new int[] {0,0,0,0,1,0,0,1,2,0,0,0,0},      new int[] {0,0,0,0,1,0,0,1,1,2,0,0,0},
        new int[] {0,0,0,0,1,0,0,1,1,1,2,0,0},      new int[] {0,0,0,0,1,0,0,1,1,1,0,2,0},
        new int[] {0,0,0,0,1,0,1,1,1,1,1,0,2},      new int[] {0,0,0,0,1,1,0,2,1,0,1,0,1},
        new int[] {0,0,0,0,2,0,1,0,1,0,1,0,1},      new int[] {0,0,0,1,1,1,0,2,0,1,0,0,0},
        new int[] {0,0,0,0,1,0,1,0,2,0,0,0,0},      new int[] {0,0,0,0,1,2,1,1,0,1,0,0,0},
        new int[] {0,0,0,0,1,0,2,0,1,0,0,0,0},      new int[] {0,0,0,0,1,2,0,1,0,0,0,0,0},
        new int[] {0,0,1,0,1,0,2,0,1,0,0,0,0},      new int[] {0,0,0,1,1,1,0,2,0,1,0,0,0},
        new int[] {0,0,0,1,1,1,1,0,2,0,1,0,0},      new int[] {0,0,0,1,1,2,0,0,0,0,0,0,0}};
    public static string[] descriptions;

    public static readonly string electronDescription =
        "The electron was discovered in 1897, preceding the discovery of the six last discovered stable elements, though it had been theorized as early as 1838. "+
        "Electrons play a role in most atoms, where they occur as electron clouds surrounding any atom containing at least one electron. In this form," +
        " electrons are delocalized, occupying not a particular point in space but occupying a region of space more or less simultaneously.\n Although" +
        " electrons play a fundamental role in the physics of all atomic chemistry, isolated electrons have useful properties too. Pure electron beams are " +
        "used in welding, lithography, particle accelerators, diffraction imaging, and in electron microscopes. These free electron beams are typically quite" +
        " dangerous as they tend to ionize atoms they come into contact with. Electrons carry the majority of electrical forces in most electrical currents, and" +
        " in beta-minus decay, an atom undergoes radioactive decay by converting a neutron into a proton, an electron, and an electron anti-neutrino.";
    public static readonly string positronDescription =
        "The positron was discovered in 1932, after all stable elements. They are the antiparticle counterpart to the electron, and have equal mass but" + 
        " opposite charge. When a positron and electron meet, they usually are converted into two or more gamma rays, though higher energy collisions " +
        " can ever produce matter. \nPositrons are extremely dangerous in concentrated form and concentrated beams are hard to produce. Most " +
        "applications of positrons produce them in small amounts from beta-plus decay, where an atom undergoes radioactive decay by converting a " +
        "proton into a neutron, a positron, and an electro neutrino. The large energies of the annihilation of the positron and electron can be used in " +
        "imaging for diagnostic purposes, and there are several radioactive isotopes used for this purpose.";
    public static readonly float[] mapX = new float[] {
                                                                           18, //n
        1,                                                                 18, //He
        1,  2,                                         13, 14, 15, 16, 17, 18, //Ne
        1,  2,                                         13, 14, 15, 16, 17, 18, //Ar
        1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, //Kr
        1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, //Xe
        1,  2,  3, //La
                    4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, //Lu
                    4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, //Rn
        1,  2,  3, //Ac
                    4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, //Lr
                    4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, //Og
        1 //Uue
    };

    public static readonly float[] mapY = new float[] {
                                                           0, //n
        1,                                                 1, //He
        2, 2,                               2, 2, 2, 2, 2, 2, //Ne
        3, 3,                               3, 3, 3, 3, 3, 3, //Ar
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, //Kr
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, //Xe
        6, 6, 6, //La
                 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, //Lu
                 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, //Rn
        7, 7, 7, //Ac
                 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, //Lr
                 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, //Og
        8    //Uue
    };

    public static readonly int[] bestN = new int[] { //from Wikipedia
                                                                            1, //n
        1,                                                                  2, //He
        3,  5,                                          5,  6,  7,  8, 10, 10, //Ne
       12, 12,                                         14, 14, 16, 16, 18, 18, //Ar
       20, 22, 24, 26, 28, 29, 30, 31, 32, 33, 34, 36, 38, 40, 42, 43, 44, 46, //Kr
       48, 50, 50, 52, 52, 54, 55, 56, 58, 59, 60, 62, 64, 67, 70, 71, 74, 75, //Xe
       78, 80, 82, //La
                   82, 82, 83, 84, 87, 90, 93, 94, 96, 98, 99,100,102,104, //Lu
                  106,108,109,110,112,114,116,118,120,122,124,126,125,125,136, //Rn
      136,138,138, //Ac
                  142,140,146,144,150,148,151,150,153,153,157,157,164,163, //Lr
                  164,163,165,166,175,175,174,178,179,178,179,179,179,177,177, //Og
      179 //Uue
    };

    public static readonly int[][] electronShell = new int[][] {
        new int[] {0,0,0,0,0,0,0,0}, new int[] {1,0,0,0,0,0,0,0}, new int[] {2,0,0,0,0,0,0,0}, //He
        new int[] {2,1,0,0,0,0,0,0}, new int[] {2,2,0,0,0,0,0,0}, new int[] {2,3,0,0,0,0,0,0}, new int[] {2,4,0,0,0,0,0,0}, //C
        new int[] {2,5,0,0,0,0,0,0}, new int[] {2,6,0,0,0,0,0,0}, new int[] {2,7,0,0,0,0,0,0}, new int[] {2,8,0,0,0,0,0,0}, //Ne
        new int[] {2,8,1,0,0,0,0,0}, new int[] {2,8,2,0,0,0,0,0}, new int[] {2,8,3,0,0,0,0,0}, new int[] {2,8,4,0,0,0,0,0}, //Si
        new int[] {2,8,5,0,0,0,0,0}, new int[] {2,8,6,0,0,0,0,0}, new int[] {2,8,7,0,0,0,0,0}, new int[] {2,8,8,0,0,0,0,0}, //Ar
        new int[] {2,8,8,1,0,0,0,0}, new int[] {2,8,8,2,0,0,0,0}, new int[] {2,8,9,2,0,0,0,0}, //Sc
        new int[] {2,8,10,2,0,0,0,0}, new int[] {2,8,11,2,0,0,0,0}, new int[] {2,8,13,1,0,0,0,0}, new int[] {2,8,13,2,0,0,0,0}, //Mn
        new int[] {2,8,14,2,0,0,0,0}, new int[] {2,8,15,2,0,0,0,0}, new int[] {2,8,16,2,0,0,0,0}, new int[] {2,8,18,1,0,0,0,0}, new int[] {2,8,18,2,0,0,0,0}, //Zn
        new int[] {2,8,18,3,0,0,0,0}, new int[] {2,8,18,4,0,0,0,0}, new int[] {2,8,18,5,0,0,0,0}, new int[] {2,8,18,6,0,0,0,0}, new int[] {2,8,18,7,0,0,0,0}, new int[] {2,8,18,8,0,0,0,0}, //Kr
        new int[] {2,8,18,8,1,0,0,0}, new int[] {2,8,18,8,2,0,0,0}, new int[] {2,8,18,9,2,0,0,0}, //Y
        new int[] {2,8,18,10,2,0,0,0}, new int[] {2,8,18,12,1,0,0,0}, new int[] {2,8,18,13,1,0,0,0}, new int[] {2,8,18,13,2,0,0,0}, //Tc
        new int[] {2,8,18,15,1,0,0,0}, new int[] {2,8,18,16,1,0,0,0}, new int[] {2,8,18,18,0,0,0,0}, new int[] {2,8,18,18,1,0,0,0}, new int[] {2,8,18,18,2,0,0,0}, //Ag
        new int[] {2,8,18,18,3,0,0,0}, new int[] {2,8,18,18,4,0,0,0}, new int[] {2,8,18,18,5,0,0,0}, new int[] {2,8,18,18,6,0,0,0}, new int[] {2,8,18,18,7,0,0,0}, new int[] {2,8,18,18,8,0,0,0}, //Xe
        new int[] {2,8,18,18,8,1,0,0}, new int[] {2,8,18,18,8,2,0,0}, new int[] {2,8,18,18,9,2,0,0}, //La
        new int[] {2,8,18,19,9,2,0,0}, new int[] {2,8,18,21,8,2,0,0}, new int[] {2,8,18,22,8,2,0,0}, new int[] {2,8,18,23,8,2,0,0}, new int[] {2,8,18,24,8,2,0,0}, //Sm
        new int[] {2,8,18,25,8,2,0,0}, new int[] {2,8,18,25,9,2,0,0}, new int[] {2,8,18,27,8,2,0,0}, new int[] {2,8,18,28,8,2,0,0}, new int[] {2,8,18,29,8,2,0,0}, //Ho
        new int[] {2,8,18,30,8,2,0,0}, new int[] {2,8,18,31,8,2,0,0}, new int[] {2,8,18,32,8,2,0,0}, new int[] {2,8,18,32,9,2,0,0}, //Lu
        new int[] {2,8,18,32,10,2,0,0}, new int[] {2,8,18,32,11,2,0,0}, new int[] {2,8,18,32,12,2,0,0}, new int[] {2,8,18,32,13,2,0,0}, //Re
        new int[] {2,8,18,32,14,2,0,0}, new int[] {2,8,18,32,15,2,0,0}, new int[] {2,8,18,32,17,1,0,0}, new int[] {2,8,18,32,18,1,0,0}, new int[] {2,8,18,32,18,2,0,0}, //Hg
        new int[] {2,8,18,32,18,3,0,0}, new int[] {2,8,18,32,18,4,0,0}, new int[] {2,8,18,32,18,5,0,0}, new int[] {2,8,18,32,18,6,0,0}, new int[] {2,8,18,32,18,7,0,0}, new int[] {2,8,18,32,18,8,0,0}, //Rn
        new int[] {2,8,18,32,18,8,1,0}, new int[] {2,8,18,32,18,8,2,0}, new int[] {2,8,18,32,18,9,2,0}, //Ac
        new int[] {2,8,18,32,18,10,2,0}, new int[] {2,8,18,32,20,9,2,0}, new int[] {2,8,18,32,21,9,2,0}, new int[] {2,8,18,32,22,9,2,0}, new int[] {2,8,18,32,24,8,2,0}, //Pu
        new int[] {2,8,18,32,25,8,2,0}, new int[] {2,8,18,32,25,9,2,0}, new int[] {2,8,18,32,27,8,2,0}, new int[] {2,8,18,32,28,8,2,0}, new int[] {2,8,18,32,29,8,2,0}, //Es
        new int[] {2,8,18,32,30,8,2,0}, new int[] {2,8,18,32,31,8,2,0}, new int[] {2,8,18,32,32,8,2,0}, new int[] {2,8,18,32,32,8,3,0}, //Lr
        new int[] {2,8,18,32,32,10,2,0}, new int[] {2,8,18,32,32,11,2,0}, new int[] {2,8,18,32,32,12,2,0}, new int[] {2,8,18,32,32,13,2,0}, //Bh
        new int[] {2,8,18,32,32,14,2,0}, new int[] {2,8,18,32,32,15,2,0}, new int[] {2,8,18,32,32,16,2,0}, new int[] {2,8,18,32,32,17,2,0}, new int[] {2,8,18,32,32,18,2,0}, //Cn
        new int[] {2,8,18,32,32,18,3,0}, new int[] {2,8,18,32,32,18,4,0}, new int[] {2,8,18,32,32,18,5,0}, new int[] {2,8,18,32,32,18,6,0}, new int[] {2,8,18,32,32,18,7,0}, new int[] {2,8,18,32,32,18,8,0}, //Og
        new int[] {2,8,18,32,32,18,8,1}};
    public static readonly int[] shellSize = new int[] {2, 8, 18, 32, 32, 18, 8, 2};

    /*public static readonly Color[] mapColors = new Color[]
    {
        new Color(0.25f, 0.0f, 0.0f, 1.0f),
        new Color(0.5f, 0.0f, 0.0f, 1.0f),
        new Color(0.75f, 0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.25f, 0.0f, 1.0f),
        new Color(1.0f, 0.5f, 0.0f, 1.0f),
        new Color(1.0f, 0.75f, 0.0f, 1.0f),
        new Color(1.0f, 1.0f, 0.0f, 1.0f),
        new Color(0.7f, 1.0f, 0.0f, 1.0f),
        new Color(0.4f, 1.0f, 0.0f, 1.0f),
        new Color(0.0f, 1.0f, 0.0f, 1.0f),
        new Color(0.0f, 1.0f, 0.4f, 1.0f),
        new Color(0.0f, 1.0f, 0.7f, 1.0f),
        new Color(0.0f, 1.0f, 1.0f, 1.0f),
        new Color(0.0f, 0.7f, 1.0f, 1.0f),
        new Color(0.0f, 0.4f, 1.0f, 1.0f),
        new Color(0.0f, 0.0f, 1.0f, 1.0f),
        new Color(0.0f, 0.0f, 0.0f, 1.0f)
    };*/

    public static readonly Color[] mapColors = new Color[] //new colors, darkened to allow bright text overlays
    {
        new Color(0.2f, 0.0f, 0.0f, 1.0f),
        new Color(0.4f, 0.0f, 0.0f, 1.0f),
        new Color(0.6f, 0.0f, 0.0f, 1.0f),
        new Color(0.8f, 0.0f, 0.0f, 1.0f),
        new Color(0.8f, 0.2f, 0.0f, 1.0f),
        new Color(0.8f, 0.4f, 0.0f, 1.0f),
        new Color(0.8f, 0.6f, 0.0f, 1.0f),
        new Color(0.8f, 0.8f, 0.0f, 1.0f),
        new Color(0.55f, 0.8f, 0.0f, 1.0f),
        new Color(0.3f, 0.8f, 0.0f, 1.0f),
        new Color(0.0f, 0.8f, 0.0f, 1.0f),
        new Color(0.0f, 0.8f, 0.25f, 1.0f),
        new Color(0.0f, 0.8f, 0.5f, 1.0f),
        new Color(0.0f, 0.8f, 0.8f, 1.0f),
        new Color(0.0f, 0.55f, 0.8f, 1.0f),
        new Color(0.0f, 0.3f, 0.8f, 1.0f),
        new Color(0.0f, 0.0f, 0.8f, 1.0f),
        new Color(0.0f, 0.0f, 0.0f, 1.0f)
    };

    public static readonly Color[] shellLabelColors = new Color[] {
        new Color(1.0f, 0.3f, 0.5f),
        new Color(1.0f, 1.0f, 0.5f),
        new Color(0.2f, 0.5f, 1.0f),
        new Color(0.3f, 1.0f, 0.3f),
        new Color(0.0f, 0.5f, 1.0f),
        new Color(1.0f, 1.0f, 1.0f),
    };

    public static readonly float[] radii = new float[] {
     10f,  25f,  15f, 145f, 105f,  85f,  70f,  65f,  60f,  50f,  45f, 180f, 150f, 125f, 110f, 100f, 100f, 100f, 90f, //Ar
    220f, 180f, 160f, 140f, 135f, 140f, 140f, 140f, 135f, 135f, 135f, 135f, 130f, 125f, 115f, 115f, 115f, 105f, //Kr
    235f, 200f, 180f, 155f, 145f, 145f, 135f, 130f, 135f, 140f, 160f, 155f, 155f, 145f, 145f, 140f, 140f, 130f, //Xe
    260f, 215f, 195f, 185f, 185f, 185f, 185f, 185f, 185f, 180f, 175f, 175f, 175f, 175f, 175f, 175f, 175f, //Lu
    155f, 145f, 135f, 135f, 130f, 135f, 135f, 135f, 150f, 190f, 180f, 160f, 190f, 140f, 130f, //Rn
    270f, 215f, 195f, 180f, 180f, 175f, 175f, 175f, 175f, 170f, 170f, 170f, 170f, 170f, 170f, 170f, 170f, //Lr
    145f, 135f, 125f, 125f, 120f, 125f, 125f, 125f, 140f, 180f, 170f, 160f, 150f, 140f, 135f, //118
    250f, 250f  //Ubn
    };

    public static readonly double[] meltingPoints = new double[] {
        0, 14.01, 0, //He
        453.69, 1560, 2349, 3915, 63.15, 54.36, 53.53, 24.56, //Ne
        370.87, 923, 933.47, 1687, 317.3, 388.36, 171.6, 83.8, //Ar
        336.53, 1115, 1814, 1941, 2183, 2180, 1519, 1811, 1768, 1728, 1357.77, 692.88, //Zn
        302.9146, 1211.4, 1090, 453, 265.8, 115.79, //Kr
        312.46, 1050, 1799, 2128, 2750, 2896, 2430, 2607, 2237, 1828.05, 1234.93, 594.22, //Cd
        429.75, 505.08, 903.78, 722.66, 386.85, 161.4, //Xe
        301.59, 1000, 1193, 1068, 1208, 1297, 1315, 1345, 1099, 1585, 1629, 1680, 1734, 1802, 1818, 1097, 1925, //Lu
        2506, 3290, 3695, 3459, 3306, 2719, 2041.4, 1337.33, 234.43, 577, 600.61, 544.7, 527, 575, 202, //Rn
        300, 973, 1323, 2115, 1841, 1405.3, 917, 912.5, 1449, 1613, 1259, 1173, 1133 //Es
    };

    public static Color MeltingColor(int Z)
    {
        return TemperatureToRGB(meltingPoints[Z] + 1);
    }

    public static Color BoilingColor(int Z)
    {
        return TemperatureToRGB(boilingPoints[Z]);
    }

    public static readonly double[] boilingPoints = new double[] {
        0, 20.28, 4.22, //He
        1560, 2742, 4200, 3915, 77.36, 90.2, 85.03, 27.07, //Ne
        1156, 1363, 2792, 3538, 550, 717.87, 239.11, 87.3, //Ar
        1032, 1757, 3109, 3560, 3680, 2944, 2334, 3134, 3200, 3186, 2835, 1180, //Zn
        2477, 3106, 887, 958, 332, 119.93, //Kr
        961, 1655, 3609, 4682, 5017, 4912, 4538, 4423, 3968, 3236, 2435, 1040, //Cd
        2345, 2875, 1860, 1261, 457.4, 165.03, //Xe
        944, 2170, 3737, 3716, 3793, 3347, 3273, 2067, 1802, 3546, 3503, 2840, 2993, 3141, 2223, 1469, 3675, //Lu
        4876, 5731, 5828, 5869, 5285, 4701, 4098, 3129, 629.88, 1746, 2022, 1837, 1235, 610, 211.3, //Rn
        950, 2010, 3471, 5061, 4300, 4404, 4273, 3501, 2880, 3383, 2900, 1743, 1269 //Es
    };

    public static readonly double[] electronAffinities = new double[] {
        -500, 73, -50, //He
        60, -50, 27, 122, -0.07, 141, 328, -120, //Ne
        63, -40, 42, 134, 72, 200, 349, -96, //Ar
        48, 2, 18, 8, 51, 65, -50, 15, 64, 112, 119, -60, 41, 119, 78, 195, 325, -60, //Kr
        47, 5, 30, 41, 89, 72, 53, 101, 110, 54, 126, -70, 29, 107, 101, 190, 295, -80, //Xe
        46, 14, 45, 635, 93, 185, 12, 16, 83, 13, 112, 34, 33, 30, 99, -2, 33, //Lu
        2, 31, 79, 14, 106, 151, 205, 223, -50, 36, 34, 91, 183, 222, -70, //Rn
        47, 10, 34, 113, 53, 51, 46, -48, 10, 27, -165, -97, -29, 34, 94, -223, -30, //Lr
        -50, -20, 20, -40, 50,  100, 150, 150, -100, 65, -120, 30, 60, 212, 5, //Og
        58 //Uue
    };

    public static readonly string[] names = new string[]
    {
        "n",
        "H",                                                                                   "He",
        "Li","Be",                                                      "B", "C", "N", "O", "F","Ne",
        "Na","Mg",                                                     "Al","Si", "P", "S","Cl","Ar",
        "K","Ca",    "Sc","Ti", "V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr",
        "Rb","Sr",    "Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te", "I","Xe",
        "Cs","Ba",
                   "La","Ce","Pr","Nd","Pm","Sm","Eu","Gd","Tb","Dy","Ho","Er","Tm","Yb",
                     "Lu","Hf","Ta", "W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb","Bi","Po","At","Rn",
        "Fr","Ra",
                   "Ac","Th","Pa", "U","Np","Pu","Am","Cm","Bk","Cf","Es","Fm","Md","No",
                     "Lr","Rf","Db","Sg","Bh","Hs","Mt","Ds","Rg","Cn","Nh","Fl","Mc","Lv","Ts","Og",
                     "Uue", "Ubn"
    };

    public static readonly string[] decaynames = new String[]
    {
        "B-", "B+", "n", "p", //3
        "2n", "2p", "3n", "3p", //7
        "A", "B-A", "B+A", "B-n", //11
        "B-2n", "B-3n", "B-p", "B+p", //15
        "B+2p", "B-t", "EC", "SF", //19
        "B-SF", "B+SF", "X- -> X + e-"  //22

    };

    public static readonly double[,] halflives = new double[,] {
            {-1, 613.9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-1, -1, 388800000, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, -1, -1, 1e-06, 0.8069, 1e-06, 0.1191, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {1e-06, 1e-06, 1e-06, -1, -1, 0.8394, 0.1783, 1e-06, 0.00875, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 1e-06, 1e-06, 4598000, 1e-06, -1, 47650000000000, 13.76, 0.0215, 1e-06, 0.00435, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 1e-06, 1e-06, 0.77, 1e-06, -1, -1, 0.0202, 0.01733, 0.0125, 0.00993, 1e-06, 0.00508, 1e-06, 0.00292, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 1e-06, 0.1265, 19.3, 1222, -1, -1, 179900000000, 2.449, 0.747, 0.193, 0.092, 0.0462, 0.016, 1e-06, 0.0062, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 1e-06, 1e-06, 0.011, 597.9, -1, -1, 7.13, 4.173, 0.6192, 0.336, 0.136, 0.084, 0.023, 0.0139, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 1e-06, 0.00858, 70.62, 122.2, -1, -1, -1, 26.47, 13.51, 3.42, 2.25, 0.097, 0.0774, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 64.37, 6584, -1, 11.16, 4.158, 4.23, 2.23, 0.384, 0.08, 0.0082, 0.0049, 1e-06, 0.0025, 1e-06, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1e-06, 1e-06, 0.1092, 1.664, 17.27, -1, -1, -1, 37.14, 202.8, 0.602, 0.197, 0.0315, 0.02, 0.0147, 0.00722, 0.0034, 0.0035, 1e-06, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.4479, 22.42, 82100000, -1, 53850, 59.1, 1.071, 0.301, 0.0305, 0.0441, 0.0484, 0.01735, 0.0129, 0.0082, 0.0055, 0.0015, 1e-06, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 1e-06, 0.093, 0.1186, 3.876, 11.32, -1, -1, -1, 566.1, 75290, 1.3, 0.313, 0.236, 0.086, 0.0905, 0.02, 0.07, 0.0039, 0.008, 0.001, 1e-06, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.0911, 0.47, 2.053, 7.183, 22630000000000, -1, 134.7, 393.6, 3.62, 0.644, 0.033, 0.0417, 0.0563, 0.0372, 0.09, 0.0115, 0.009, 0.0076, 0.01, 0.002, 0.001, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0.029, 0.0423, 0.14, 0.22, 2.245, 4.15, -1, -1, -1, 9442, 4828000000, 6.18, 2.77, 0.78, 0.45, 0.09, 0.09, 0.0475, 0.033, 0.02, 0.0125, 0.015, 0.01, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 0.0437, 0.26, 0.2703, 4.142, 149.9, -1, 1233000, 2190000, 12.43, 47.3, 5.6, 2.31, 0.64, 0.282, 0.15, 0.101, 0.0485, 0.0358, 0.0185, 0.008, 0.004, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.0155, 0.125, 0.188, 1.176, 2.553, -1, -1, -1, 7549000, -1, 303, 10220, 11.5, 8.8, 1.99, 1.016, 0.265, 0.1, 0.068, 0.05, 0.02, 0.01, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.19, 0.298, 2.504, 1.527, -1, 9508000000000, -1, 2234, 3372, 81, 38.4, 6.8, 3.13, 0.56, 0.413, 0.232, 0.101, 0.1, 0.05, 0.02, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.0151, 0.098, 0.173, 0.8438, 1.776, -1, 3025000, -1, 8489000000, -1, 6577, 1038000000, 322.2, 712.2, 21.48, 8.4, 1.23, 0.415, 0.236, 0.106, 0.06, 0.01, 0.003, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.178, 0.341, 1.236, 458.2, -1, 3.938e+16, -1, 44480, 80280, 1328, 1068, 105, 17.5, 6.8, 1.26, 0.472, 0.365, 0.11, 0.03, 0.01, 0.003, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.0257, 0.1012, 0.1811, 0.4437, 0.8603, -1, 3137000000000, -1, -1, -1, 14050000, -1, 391900, -1, 523.1, 13.9, 10, 4.6, 0.461, 0.09, 0.022, 0.011, 0.005, 0.003, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 0.1823, 0.5963, 0.6808, 14010, 14550, -1, 7240000, 289400, 157200, 3431, 102.5, 12.4, 8.2, 2.4, 0.526, 0.096, 0.026, 0.022, 0.012, 0.01, 0.003, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.0285, 0.0524, 0.0819, 0.2087, 0.509, 1865000000, 11090, -1, -1, -1, -1, -1, 345.6, 102, 32.7, 2.1, 1.3, 0.2, 0.095, 0.055, 0.0285, 0.0222, 0.015, 0.01, 0.003, 0.004, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.0793, 0.111, 0.547, 0.4226, 1956, 1380000, 28510000, 4.734e+24, -1, 224.6, 92.58, 49.8, 6.54, 0.216, 0.35, 0.191, 0.095, 0.122, 0.0482, 0.0336, 0.0196, 0.015, 0.01, 0.005, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0133, 0.0211, 0.0428, 0.0609, 0.2243, 0.5, 77620, 2538, -1, 2393000, -1, -1, -1, 209.8, 356.4, 21.1, 7, 1.05, 0.49, 0.243, 0.206, 0.129, 0.043, 0.0275, 0.0238, 0.01, 0.005, 0.002, 0.001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 0.0362, 0.088, 0.1581, 0.382, 0.2832, 2772, 483100, 116800000000000, 26970000, -1, 9284, 85.4, 3, 4.59, 0.28, 0.709, 0.092, 0.275, 0.0888, 0.0919, 0.0642, 0.0467, 0.0337, 0.0221, 0.0199, 0.005, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0022, 0.013, 0.0219, 0.0453, 0.0647, 0.1521, 0.3054, 29790, 510.6, -1, 86590000, -1, -1, -1, 3844000, 82680000000000, 358.8, 68, 6.1, 2, 0.81, 0.351, 0.394, 0.188, 0.1082, 0.063, 0.0337, 0.019, 0.0129, 0.002, 0.002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.0388, 0.0688, 0.1111, 0.242, 0.1933, 63110, 6673000, 23470000, 6122000, -1, 166300000, 5936, 92.4, 26.9, 0.3, 1.16, 0.194, 0.329, 0.2, 0.18, 0.112, 0.08, 0.0525, 0.0407, 0.0313, 0.0265, 0.023, 0.015, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0028, 0.0075, 0.0185, 0.0238, 0.0418, 0.0552, 0.1142, 0.2047, 524900, 128200, -1, 2556000000000, -1, -1, -1, 3194000000, -1, 9063, 196600, 21, 29, 11.4, 6, 2.56, 1.57, 0.84, 0.5077, 0.3316, 0.2346, 0.1589, 0.1222, 0.044, 0.03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.057, 0.093, 0.1963, 3.204, 81.5, 1422, 12020, 580.2, -1, 45720, -1, 307.2, 222600, 30.9, 171, 44.5, 19.4, 6.63, 4.2, 1.63, 1.224, 0.6377, 0.4679, 0.3307, 0.241, 0.1133, 0.0732, 0.05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0018, 0.0198, 0.0329, 0.038, 0.0867, 0.182, 142.8, 89.1, 33090, 2308, -1, 21080000, -1, -1, -1, 3384, -1, 147, 167400, 23.5, 95.6, 10.2, 5.7, 2.08, 1.47, 0.746, 0.5622, 0.3032, 0.1779, 0.119, 0.05, 0.05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 0.07, 0.167, 0.1161, 32.4, 157.6, 912, 33490, 281800, 4071, -1, 1268, -1, 50490, 17500, 487.2, 126, 32.6, 13.2, 5.09, 2.848, 1.9, 1.217, 0.599, 0.3081, 0.085, 0.0922, 0.047, 0.01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.008, 0.03, 0.044, 0.129, 0.142, 63.7, 30.9, 8136, 1134, 23410000, 140600, -1, 987600, -1, -1, -1, 4967, -1, 40360, 5280, 18.98, 29.5, 8, 4.56, 1.85, 0.951, 0.494, 0.226, 0.15, 0.1, 0.05, 0.05, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 0.04, 0.17, 0.09577, 42.5, 151.6, 912, 3156, 235100, 93600, 6938000, 1535000, -1, 93120, 139600, 5442, 540.6, 15.2, 33.3, 19.1, 13.4, 4.02, 2.021, 0.945, 0.492, 0.27, 0.2, 0.08, 0.05, 0.03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.03, 0.033, 0.033, 0.133, 35.5, 27.4, 2466, 284.4, 725800, 25740, -1, 10350000, -1, -1, -1, 10320000000000, -1, 1107, -1, 1335, 195.6, 32.9, 14.3, 5.5, 1.53, 0.43, 0.21, 0.27, 0.1, 0.05, 0.02, 0.01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.0791, 21.4, 78.6, 204, 1524, 5802, 58320, 205300, 387, -1, 1061, -1, 127000, 8546, 1906, 174, 55.1, 55.65, 16.34, 4.357, 1.91, 0.543, 0.314, 0.152, 0.07, 0.05, 0.02, 0.01, 0.005, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.028, 0.052, 0.1, 17.16, 27.3, 690, 276, 53280, 4464, -1, 126100, -1, 7227000000000, -1, -1, -1, 338900000, -1, 4578, 10170, 189, 32.32, 8.57, 1.84, 1.286, 0.212, 0.114, 0.08, 0.0622, 0.0428, 0.04, 0.012, 0.005, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.06478, 19, 36.5, 226.8, 1060, 1374, 33.4, 16460, 76.38, 7448000, 2836000, -1, 1611000, 1.568e+18, 1066, 919.2, 158, 58.2, 4.48, 5.84, 2.702, 0.3777, 0.201, 0.1691, 0.114, 0.0564, 0.048, 0.0318, 0.037, 0.026, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.025, 0.027, 0.088, 7.89, 9, 156.1, 135, 6378, 1338, 2191000, 116700, -1, 5603000, -1, -1, -1, 4369000, 908500000, 34740, 9400, 445.8, 75.3, 23.9, 1.07, 0.429, 0.653, 0.269, 0.202, 0.1138, 0.069, 0.053, 0.0506, 0.039, 0.021, 0.01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0001, 0.00012, 0.063, 0.054, 14.8, 30.1, 70.4, 8.3, 424.8, 2370, 9648, 53060, 287300, 9212000, -1, 230400, 5055000, 12740, 36650, 1122, 618, 5.34, 3.75, 0.548, 1.484, 0.735, 0.426, 0.298, 0.239, 0.197, 0.095, 0.074, 0.0335, 0.03, 0.025, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0001, 0.05, 0.056, 4.6, 5.5, 32, 42, 1548, 471.6, 59400, 6048, 7206000, 282300, -1, -1, -1, 50810000000000, -1, 5532000, -1, 60300, 30.7, 2.1, 7.1, 2.3, 2.9, 1.38, 0.92, 0.67, 0.1786, 0.1457, 0.0785, 0.056, 0.0375, 0.024, 0.043, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.05, 3.9, 9.8, 20.5, 88, 222, 870, 7308, 52560, 21460000000, 1.095e+15, -1, 643800000000, 3023000, 84060, 4326, 2.86, 15, 1.5, 7.1, 4.3, 1.5, 4.9, 2.95, 1.05, 0.289, 0.198, 0.1069, 0.082, 0.054, 0.038, 0.032, 0.017, 0.023, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.03, 0.023, 2.3, 3.2, 19.1, 14.1, 480, 126.6, 20020, 929.4, -1, 126200000000, -1, -1, -1, -1, -1, 237500, -1, 876.6, 678, 67.5, 60, 35.6, 8.73, 3.5, 1.105, 0.7, 0.292, 0.1936, 0.125, 0.08, 0.058, 0.0455, 0.032, 0.022, 0.021, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 0.055, 2.2, 6.4, 12.8, 49.2, 188.4, 255, 9900, 17580, 72000, 369800, 132900000000000, 132500000000000, 6662000000000, 15.46, 853.2, 5.28, 54.2, 1098, 456, 35.6, 21.2, 5.17, 1.14, 0.9, 0.35, 0.323, 0.152, 0.09, 0.078, 0.057, 0.0445, 0.03, 0.022, 0.021, 0.022, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.05, 0.05, 1.3, 1.5, 11, 8, 219, 59.7, 3108, 5915, -1, 245100, -1, -1, -1, -1, -1, 3391000, -1, 15980, 32120000, 225, 273, 34.5, 12.04, 2.12, 1.75, 0.8, 0.54, 0.318, 0.204, 0.151, 0.099, 0.0695, 0.045, 0.029, 0.025, 0.019, 0.015, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.01, 0.015, 1.6, 4.66, 13.9, 70.6, 301.2, 594, 1842, 523.2, 1391000, 74880, 104100000, 17880000, -1, 42.3, 127300, 30.07, 1302, 16.8, 80, 3.35, 11, 3.4, 2.8, 1.85, 0.99, 0.685, 0.421, 0.284, 0.19, 0.1296, 0.076, 0.051, 0.042, 0.03, 0.0265, 0.019, 0.028, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 1.1, 1.15, 9, 7.5, 122, 186, 1062, 1284, 313600, 30490, -1, 1468000, -1, -1, -1, 205100000000000, -1, 49320, -1, 1404, 75740, 93, 145.2, 25, 11.8, 4.3, 1.9, 0.92, 0.492, 0.29, 0.195, 0.108, 0.088, 0.057, 0.0486, 0.038, 0.035, 0.031, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.02, 0.037, 1.76, 4.44, 25.5, 47.5, 124.2, 120.6, 666, 774, 3942, 4152, 3567000, 1438, -1, 142.9, -1, 24.56, 642200, 11270, 19330, 4.6, 1200, 229.8, 73.6, 3.76, 6, 1.52, 0.78, 0.529, 0.3, 0.1779, 0.159, 0.0993, 0.089, 0.059, 0.0499, 0.0406, 0.035, 0.03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.08, 0.09, 0.88, 1.1, 9.2, 16, 49.1, 81.6, 330, 438, 3462, 3330, -1, 23400, -1, 39880000, -1, -1, -1, 2.537e+23, -1, 192500, -1, 8964, 3018, 161.4, 50.8, 13.5, 5.24, 2.1, 1.25, 0.68, 0.513, 0.33, 0.246, 0.1515, 0.1268, 0.098, 0.082, 0.061, 0.065, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.05, 0.037, 3.1, 5.83, 15.1, 23.3, 60, 108, 304.2, 372, 1944, 3480, 15000, 17710, 242500, 892.8, -1, 71.9, 1.392e+22, 14.1, 2592, 5, 144, 3.08, 23.1, 1.5, 6.17, 3.12, 2.36, 1.53, 1.09, 0.816, 0.57, 0.284, 0.261, 0.198, 0.165, 0.14, 0.101, 0.086, 0.07, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.005, 1.16, 1.97, 3.8, 7, 20.8, 34, 115.2, 174, 618, 1080, 14950, 2118, -1, 9944000, -1, -1, -1, -1, -1, -1, -1, 97310, -1, 11160000, -1, 832900, 7258000000000, 7560, 3544, 133.8, 223.2, 56, 39.7, 1.46, 0.89, 0.515, 0.35, 0.273, 0.15, 0.13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.47, 1.12, 0.6, 4, 7.4, 17, 23.6, 75, 53.5, 400.2, 209.4, 1926, 948, 10080, 216, 137500, 953.4, -1, 235300, -1, 5201000, 87050000, 1067000, 332600, 32580, 15720, 2370, 1382, 167.4, 140.4, 0.78, 1.679, 0.923, 0.484, 0.348, 0.093, 0.1, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 7.8e-05, 0.0031, 2.1, 4.6, 18.6, 26.2, 120, 102, 912, 348, 8964, 3720, 518400, 57780, -1, 1656000, -1, -1, -1, -1, -1, 33660, -1, 4176, -1, 1500, 276800, 750, 2508, 19, 17.63, 2.49, 1.4, 0.5, 0.3, 0.15, 0.1, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2e-05, 0.036, 0.000103, 0.664, 2.5, 3.34, 6.6, 2.1, 78, 2.91, 133.2, 822, 1146, 4900, 7632, 217.8, 47600, 360800, 5133000, 1117000, -1, 1499, 495400000000000, 44500, 693400, 8262, 74990, 3150, 23690, 83.4, 24.13, 6.23, 2.282, 0.86, 0.43, 0.222, 0.13, 0.1, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.013, 0.093, 0.74, 2.7, 2.74, 10, 18, 59, 61, 228, 348, 2760, 2406, 72360, 7488, -1, 60840, -1, 3140000, -1, -1, -1, -1, -1, 453400, -1, 32900, -1, 229.1, 848.4, 39.68, 13.6, 1.73, 1.23, 0.511, 0.388, 0.188, 0.146, 0.13, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 0.00049, 1.77e-05, 0.57, 1.4, 0.7, 8.4, 14, 43, 60.4, 155, 21.18, 352.8, 30.9, 2802, 98.4, 22500, 218.4, 115400, 1753, 837100, 559900, -1, 65170000, 41970000000000, 1137000, 949200000, 2005, 556.2, 63.7, 24.84, 1.684, 1.791, 0.994, 0.582, 0.323, 0.23, 0.145, 0.113, 0.0844, 0.069, 0.03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.1, 0.46, 0.45, 1.3, 1.75, 5.2, 5.4, 24, 29.7, 117, 162, 660, 198, 6000, 762, 210000, 8028, -1, 995300, -1, 333000000, -1, -1, -1, -1, -1, 4988, 1102000, 1096, 636, 14.5, 11.5, 4.31, 2.22, 0.894, 0.62, 0.348, 0.259, 0.167, 0.139, 0.116, 0.053, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.0217, 0.2, 1, 2.8, 5.3, 8.6, 17, 29.21, 64.8, 54, 306, 310.8, 696, 522, 3540, 17280, 14080, 387, 70200, 592.2, 1893000000000, 3.219e+18, -1, 145000, 14110, 5466, 852, 40.8, 24.8, 6.27, 4.06, 1.35, 1.07, 0.504, 0.465, 0.287, 0.245, 0.161, 0.101, 0.084, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.2, 0.25, 1.1, 2, 3.8, 9.1, 9.7, 51, 34, 235.8, 210, 1374, 618, 12640, 5820, 273000, 63720, -1, 32400, -1, 11890000, -1, 2809000, -1, 118900, 24620000, 180.6, 811.2, 56.4, 56.8, 4.94, 6.05, 1.76, 1.42, 0.865, 0.722, 0.313, 0.233, 0.175, 0.099, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.012, 0.5, 0.8, 1.2, 3.3, 3.12, 4.2, 2.85, 30, 40, 90, 89.4, 390, 1020, 1440, 786, 4608, 87, 15880, 203.4, -1, 68830, 1172000, 1037, 21540, 1449, 804, 137.4, 135.6, 6.19, 18.9, 3.57, 4.28, 2.3, 1.47, 0.444, 0.307, 0.181, 0.134, 0.17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0.65, 1, 1.8, 5, 6.8, 21, 25.4, 93.6, 70, 510, 744, 3042, 2310, 18140, 1782, 291200, 8964, -1, -1, 7.227e+22, -1, -1, 948700, -1, 6221, -1, 746.4, 684, 31.6, 25.9, 8.9, 5.06, 1.15, 0.81, 0.5, 0.439, 0.215, 0.31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 1, 1, 2.4, 2.6, 6.3, 6.2, 13.5, 22, 49, 107, 120, 10, 249, 9.2, 1254, 40.5, 22900000, 31360000, 558600000, 174500000, 82790000, 463800, 191100, 9713, 102200, 247.2, 315, 160.8, 41.5, 27.2, 10.56, 4.8, 1.49, 0.725, 1.05, 0.63, 0.43, 0.2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0.55, 1, 1.2, 4, 2.89, 9.5, 10.3, 47, 45, 186, 154.2, 889.2, 612, 4349, 525, -1, 29380000, 2.146e+15, 3.364e+18, 1.988e+23, -1, -1, 2840000000, -1, 166600, -1, 1338, 33840, 481.8, 318, 11.37, 9.6, 4.8, 2.7, 1.3, 1.43, 0.98, 0.8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 0.0178, 0.1, 0.2, 0.5, 1.5, 3.3, 8.4, 12.1, 17.9, 1.51, 40.7, 2.36, 155.4, 10.2, 512400, 398300, 2082000, 4709000, 8044000, 1164000000, -1, 426600000, -1, 271400000, 149600000, 1312000, 54650, 2754, 1086, 42.4, 26.2, 11, 7.7, 4.15, 2.53, 1.24, 1.33, 0.2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.4, 1.1, 1, 2.2, 4.7, 5.7, 15.8, 14, 70.2, 39, 268.2, 1380, 4171000, 137000, 2237000000, 801800, 56490000000000, 10700000, 3.408e+21, 20770000, -1, -1, -1, -1, -1, 66520, -1, 218.8, 504, 68, 45, 11, 5.1, 4.2, 3.03, 0.75, 0.42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00101, 0.2, 0.6, 0.8, 1.6, 2.32, 3.5, 0.597, 12, 1, 30.9, 8, 5904, 3600, 14820, 12530, 63390, 63000, 202200, 77400, 459600, 462200, 2241000000, 5680000000, -1, 6247000, 595300, 456, 1170, 180, 126.6, 27.1, 18.9, 9.4, 5.13, 0.96, 1.23, 0.76, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.2, 0.6, 0.7, 0.9, 2.3, 5.6, 9.1, 9.5, 33.2, 67, 198, 252, 430.2, 1074, 8568, 23040, 94670000000000, 35640, -1, 29300, -1, 12480000, -1, -1, -1, -1, -1, 8402, 293800, 372, 522, 39, 54.9, 4.07, 3.4, 1.43, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.006, 0.0041, 0.4, 0.3, 0.7, 2.4, 2.8, 5.8, 2.2, 21.1, 76.8, 35.2, 161.8, 120.6, 705.6, 2880, 3360, 756, 678, 1983, 1536, 8928, 900, 144200000000, 1740, -1, 96570, 11160, 179.4, 283.2, 165.6, 53, 25, 6.9, 3.2, 1.88, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-05, 0.2, 0.4, 0.9, 1.7, 3.2, 4.6, 4, 18.5, 23.5, 10.3, 37.1, 223.8, 318, 1170, 1119, 8244, 2160, 102900, 11560, -1, 4500, -1, 37300, -1, -1, -1, 811500, -1, 27060, 177500, 86.04, 192, 72, 20, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.3e-06, 3.17e-06, 0.155, 0.58, 0.7, 0.9, 3, 4.17, 8, 1.48, 8.1, 21.6, 83.8, 217.8, 238.8, 547.8, 564, 1812, 1302, 6516, 120, 108200, 27720, 799200, 8044000, -1, 11110000, 60590000, 229000, 29660, 324, 912, 111, 90, 30, 20, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0.7, 0.7, 1.6, 3.03, 4.2, 0.409, 1.793, 26.1, 38.6, 89.4, 100.2, 288, 252, 1132, 663, 4548, 594, 204100, 1050, -1, 2766000, -1, -1, -1, -1, -1, 361600, -1, 6880, 4440, 480, 144, 60, 10, 3, 1, 0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.045, 0.0784, 0.65, 0.9, 1, 0.0686, 0.494, 6.8, 10.6, 12.1, 36.1, 77, 82.2, 238.2, 188.4, 644.4, 159, 3090, 330, 122600, 173800, 711900, 578900, 43230000, 104500000, -1, 1.163e+18, 574200, 1704, 16520, 342, 210, 120, 58, 20, 6, 2, 1, 0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.4, 2, 0.84, 0.023, 0.115, 0.99, 5.2, 13.6, 18.4, 39.4, 40, 111, 76, 406.2, 123, 1557, 194.4, 57640, 43560, 59010000, 84960, 6.311e+22, 6104000, -1, -1, -1, -1, -1, 3662000, 280900000000000, 3665, 14830, 210, 156, 30, 20, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0032, 0.106, 0.0101, 0.049, 1.04, 1.7, 3, 3.57, 10.6, 14.2, 31, 34.4, 79.8, 120, 294, 405.6, 1398, 2208, 11300, 4104, 37800, 29120, 203600, 8496, 57430000, 29350, -1, 9914000, 440600, 31320, 2964, 630, 138, 19.6, 3, 5.3, 3, 2.2, 0.5, 0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.275, 0.00125, 0.0082, 0.09, 0.409, 1.19, 2.63, 6.3, 5.1, 19.2, 19.9, 50.9, 74, 145.2, 142.8, 396, 456, 1992, 2112, 9000, 7920, 1866000, 2223, -1, 10470000, -1, -1, -1, 6489000, -1, 86400, 6029000, 642, 1800, 45, 60, 3, 5, 3, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4e-05, 0.000611, 0.00044, 0.107, 0.39, 0.719, 2.62, 2.25, 3.4, 4.4, 8.1, 9.2, 15.2, 15, 120, 144, 353.4, 318, 840, 792, 1170, 147.6, 71640, 231100, 6048000, 3059000, -1, 321300, 1.366e+18, 61210, 87480, 186, 588, 16, 20, 5, 6, 2.4, 0.3, 0.3, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00064, 0.0021, 0.0055, 0.021, 0.071, 0.213, 0.839, 2.1, 3.46, 7.37, 8.3, 19.2, 22.4, 44, 84, 216, 180, 300, 390, 1290, 6300, 78620, 46800, -1, 8031000, 6.311e+22, -1, -1, -1, -1, 1295000, -1, 107400, 189300000, 390, 2094, 168, 60, 6, 7, 1, 0.2, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.001, 1e-06, 0.0105, 0.0293, 0.23, 0.353, 0.91, 3.1, 4.4, 9, 7.9, 9, 8.7, 30, 12, 79, 90, 294, 900, 3480, 11120, 51840, 59900, 37800, 149400, 1140000, 1018000, -1, 6379000, -1, 69410, 8244, 52, 348, 8, 7, 43, 21, 11, 6, 1, 0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0003, 0.0008, 0.00202, 0.00699, 0.01393, 0.0455, 0.0976, 0.382, 0.889, 2.43, 6.33, 10.6, 20.7, 21.2, 56, 52, 160.2, 390, 1038, 4254, 7488, 8460, 881300, 39130, 2.051e+19, 244500, -1, 1578000000, -1, -1, -1, 71610, -1, 1848, 45360, 150, 158400, 22, 10.3, 5, 5, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00015, 0.00029, 2.23e-05, 0.028, 0.0255, 0.139, 0.202, 1.05, 1.46, 2.6, 7.1, 8.4, 13.7, 15.5, 42.8, 20.6, 255, 642, 498, 530.4, 1722, 2568, 11450, 17780, 63540, 136900, 16070000, 532800, -1, 232800, 271200, 2904, 1560, 28.4, 60, 38.3, 32.5, 47, 10, 10, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7e-05, 0.000231, 0.0008, 0.002, 0.0106, 0.0203, 0.1273, 0.2665, 1.05, 2.59, 3.6, 10.83, 9.4, 30.87, 49.1, 82.8, 114, 195, 456, 1200, 2940, 17460, 13680, 14110000000, 38480, -1, 233800, -1, -1, -1, -1, -1, 4027000, -1, 308.4, 499.2, 174, 2520, 38, 64, 26, 60, 1, 1, 1, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0062, 0.018, 0.255, 0.265, 1.09, 3.2, 1.9, 6.9, 9.5, 19.5, 40, 51, 71, 138, 156, 1200, 576, 1296, 1980, 4176, 6624, 10220, 19080, 26710, 93960, 263000, 1064000, -1, 119400000, -1, 252.1, 286.2, 183.2, 129.7, 78, 80, 31, 24, 11, 10, 6, 1, 0.2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00023, 0.0039, 0.0041, 0.039, 0.055, 0.535, 0.49, 6.3, 4.82, 15.2, 25.1, 39, 71, 79.8, 210, 300, 642, 900, 2220, 486, 8640, 5400, 77400, 33590, 1657000000000, 186900, -1, 545900000000000, -1, -1, -1, 11640, 700600000, 2170, 38300, 612, 1624, 140.4, 99, 20, 15, 10, 30, 3.87428807396253, 12.8786808430944, 3.50955989032349, 8.31345206171263, 2.43501863820171, 5.88969499758609, 1.85716119812339, 3.67414393486183, 1.41375476697106, 2.55784170482041, 1.03450376977181, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0066, 0.002, 0.0148, 0.037, 0.06, 0.658, 6.3, 11.7, 34.6, 63.6, 95, 183, 306, 559.8, 618, 1620, 2184, 6180, 6192, 42340, 40390, 1323000, 539400, 984600000, 11610000000000, -1, 433000, 128.4, 3633, 2737, 1194, 456, 135, 98.5, 33, 8.7, 9.5, 5, 2, 1, 0.3, 3.83763252821487, 1.35214011313225, 2.75972221799659, 1.01604737332293, 1.76204863554684, 0.769219398531948, 1.21878103963691, 0.564365759204031, 0.888754937154615, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.4e-05, 0.0014, 0.000275, 0.0038, 0.00246, 0.022, 0.0322, 0.388, 0.392, 4.64, 5.56, 53.6, 105.6, 328.2, 690.6, 936, 2676, 2202, 12670, 6264, 760300, 20880, 91450000, 3913000000, 11960000, 0.516, 1e-06, 3.708e-06, 0.0001637, 0.001781, 0.145, 1.514, 185.9, 618, 40, 132, 546, 60, 60, 20, 20, 5, 20.8034213569648, 4.80307678180725, 10.6587365695422, 3.40582056769005, 6.5488033486668, 2.26556471033894, 4.19037494958121, 1.61322680226639, 2.66414125476938, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0021, 0.0115, 0.029, 0.286, 0.29, 0.388, 0.3882, 3, 7.02, 43.2, 85.2, 184, 444, 547.2, 2028, 1836, 6516, 5868, 19510, 29160, 25970, 0.314, 1e-06, 1e-06, 0.0001, 0.0003, 0.03262, 1.5, 56, 222.6, 138, 54, 50, 150, 120, 20, 20, 5, 5, 2.22310177992142, 4.16863274839751, 1.59934388971953, 2.69624182033206, 1.1149464478684, 1.87946652466888, 0.835243036535769, 1.39911711462347, 0.494209950730193, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00115, 0.00078, 0.007, 0.0047, 0.054, 0.065, 0.59, 1.09, 7, 9.7, 44, 74.52, 169.8, 340.2, 555, 1461, 1728, 8640, 52560, 1434, 0.0195, 1e-06, 2.3e-06, 4.5e-05, 0.00054, 0.03375, 3.96, 55.6, 1542, 330200, 1458, 6420, 279.6, 444, 20.2, 65, 11.9, 10, 0.3, 41.3488843593077, 9.10573453475699, 20.955243243072, 5.55293543622813, 11.7300903891847, 3.73303295493227, 7.54920793867933, 1.73243866372489, 3.84651683760733, 1.51114215807718, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00233, 0.015, 0.0066, 0.0475, 0.0628, 0.372, 0.55, 1.75, 3.82, 16, 14.8, 59.1, 50.5, 190.8, 186, 1200, 34.14, 0.00518, 1e-06, 1e-06, 1.68e-05, 0.001, 0.02, 27.4, 288.1, 852, 1320, 199.8, 237, 49, 148.2, 38, 50.2, 19.1, 17.6, 5.5, 0.9, 3.6792074544901, 6.49781472066177, 2.2785050605047, 4.00548044141575, 1.62759059889742, 3.14062221017386, 0.951844898244226, 1.71719289764838, 0.789146943728958, 1.41081311914934, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.02, 0.0041, 0.036, 0.06, 0.22, 0.24, 1.38, 1.11, 4.71, 4, 13.2, 13, 163.8, 2.437, 0.00167, 1e-06, 1.63e-06, 2.52e-05, 0.01, 0.0179, 28, 33.6, 988200, 313800, 1287000, 50490000000, 2532, 181500000, 240, 5580, 104, 240, 30, 30, 3, 92.6943213029111, 16.5869493075028, 40.0390650844271, 8.86385909111347, 19.9953862690935, 4.13357525777178, 9.25792968122275, 2.86722020860843, 7.44474556939694, 2.48251791233483, 5.21244199766881, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.08, 0.025, 0.031, 0.097, 0.094, 0.35, 0.213, 0.895, 0.738, 8.2, 0.17, 0.00044, 1e-06, 1e-06, 1.18e-05, 0.02636, 0.052, 5, 126, 10010, 857100, 105700, 687100000, 22140, 3762, 122, 450, 118.8, 145, 45, 62, 270, 240, 6.05661978378214, 12.3941242439919, 4.19966813945675, 7.06988719394924, 1.94062782237464, 4.14917064191531, 1.3960030524914, 3.02024795846711, 1.19804355892085, 2.17792519666035, 1.02837208717382, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0024, 0.06, 0.016, 0.048, 0.0317, 0.144, 0.087, 1.2, 0.026, 0.000247, 1e-06, 1.021e-06, 9.7e-06, 0.00178, 0.00224, 0.6, 1.04, 525, 1842, 1615000, 60350000, 249900000000, 2379000000000, 91870, 4.418e+17, 1310, 2082000, 432, 2238, 288, 564, 120, 251.580258450366, 36.811503644041, 113.991854756049, 10.5849522578154, 27.8335193776072, 6.66318849504447, 22.0384566238781, 5.83459238459333, 12.9492176707968, 4.33013674503408, 9.86092285686568, 3.25429436571065, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.003, 0.0075, 0.007, 0.017, 0.014, 0.105, 0.00348, 0.000113, 1e-06, 1e-06, 5.9e-06, 0.0032, 0.0051, 0.846, 1.7, 108, 2298, 79200, 129600, 1503000, 1034000000000, 114000, 2331000, 24120, 1464, 546, 522, 136.8, 6480, 120, 120, 10.1112743659659, 28.3105501869445, 5.20123608380485, 9.33862481757378, 2.9178207088045, 7.54938047403724, 2.55260177641518, 5.10887937719602, 2.00760915024871, 3.81191727870155, 1.52221354703574, 2.62410215311206, 1.06648817073195, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0014, 0.0069, 0.0008, 0.00055, 5.5e-05, 1e-06, 1e-06, 4.7e-06, 2.1e-05, 0.000396, 0.061, 0.269, 66, 546, 3468, 1748000, 362900, 2174000000, 5024000000000, 7747000000000, 2.222e+16, 739100000000000, 583400, 1.41e+17, 1407, 50760, 300, 1008, 600, 1166.65449654292, 45.1879999113121, 121.954839884128, 18.2016208910639, 67.3893818717027, 15.8820257656027, 46.95622911265, 11.5150775860636, 31.2267947985332, 7.95674967711041, 17.7931049605061, 4.587614313615, 8.55262831572201, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0.0001, 0.006, 0.035, 0.51, 61.4, 240, 276, 2928, 882, 2172, 380200, 34220000, 4828000000000, 67660000000000, 181400, 203600, 3714, 834, 132, 111, 137.4, 120, 14.5661183600329, 30.4270196107181, 8.39110189836564, 18.6320355184287, 5.91957965076318, 13.0607507996933, 4.31433798912399, 9.25270126559634, 3.1524623995921, 6.02631263092164, 2.01651965286016, 3.38391805059968, 1.49866661305687, 2.30341564275106, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.000332763659878071, 1e-06, 1e-06, 4.28807473986988e-06, 6.86878974277292e-06, 0.000763552774599826, 0.00158439621043232, 0.02, 2.1, 91, 102, 516, 2022, 1254, 31680, 1518, 90190000, 3943000, 2768000000, 760800000000, 207000000000, 452200000, 11830000000000, 17840, 2.525e+15, 37800, 936600, 196100, 1287.68907594883, 96.338137388875, 505.803347523381, 60.2064586232379, 261.19892188374, 35.7864127526449, 129.427630516455, 21.8573573259496, 64.2143897654754, 11.4745186156688, 24.600741386248, 7.09264944796286, 12.8122268630301, 5.92575490905903, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.000858176224770766, 1e-06, 1e-06, 1e-06, 2.11394115107725e-05, 0.113079366509409, 9.32625205101846e-05, 0.0124553164001937, 1.8, 40, 60, 78.6, 192, 139.2, 618, 216, 4416, 5880, 42840, 182900, 13650000000, 57670, 232400000000, 36360, 7380, 2340, 1380, 180, 60, 24.3124798064113, 71.9196790966388, 14.8673245594407, 49.2242881990133, 11.5745564581916, 29.4679948943908, 7.50596115562796, 16.2864051726973, 4.3462374053737, 7.74810759742772, 2.9212498559217, 4.56026707493197, 2.23088544707518, 5.11429844683391, 1.85398139655885, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7.38763039938861e-05, 1e-06, 1e-06, 1e-06, 2.10540392395862e-06, 1.09161828467048e-05, 1.03887314763497e-06, 3.25984081484358e-05, 1e-06, 20, 10, 27, 52, 300, 408, 1200, 7920, 9000, 2333000, 2834000, 14070000, 918300000, 571200000, 260300000000, 148500000000, 492300000000000, 10980000000000, 3849, 261900000000, 1008, 60, 226.639714577698, 3881.1203046337, 156.649489709448, 1092.52237233068, 79.3470778926787, 335.592423568445, 33.1719309015212, 82.6709846662038, 17.2336365476362, 45.2841330411186, 10.2256531356127, 41.1117177145204, 10.9939658294955, 13.9483254283799, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.00024260206329276, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 40, 20, 60, 120, 120, 144, 240, 288, 276, 420, 16560, 18070, 427700, 155500, 43550000000, 284000000, 28270000, 11560, 3336, 108, 600, 60, 245.661473349694, 41.3933200389141, 148.332279568618, 23.6522876661682, 65.1479191251433, 11.56689520661, 22.4200028500429, 6.66756457881239, 15.0792761616532, 4.32905428761653, 12.7893910649041, 4.33557088585714, 5.7904455024195, 3.1967516886649, 8.62889264152234, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.85923138465646e-05, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 6.15014468527904e-06, 3.37125718342336e-05, 0.8, 0.0211, 60, 40.3, 141, 209.4, 642, 1164, 2700, 128500, 11200, 28810000, 11080000000, 412800000, 28400000000, 83470000, 1539000, 5227000, 5100, 738, 1240.8198835089, 270229.81836531, 546.516510932587, 6912.72326974922, 147.242010102813, 526.953111882824, 51.9247704261465, 204.390335605624, 25.1652208595863, 192.596558490735, 28.2612315786911, 45.2127060572079, 16.7136414284474, 64.1422342219845, 15.2536397303153, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0.000733264898415284, 0.0326579936598257, 1, 1, 10, 17.8, 21.6, 37, 66, 450, 273, 1440, 6132, 30960, 118800, 40750000, 1769000, 23820000, 3439000, 1524, 665300, 180, 1111.87421578614, 95.1117501196344, 410.623260395129, 36.0106871718575, 87.3566224468612, 16.4826428121644, 47.8798707226203, 8.82908454847359, 39.2393493440886, 9.87576607706457, 15.4809487075011, 6.86490121766762, 13.4947408035314, 5.5475838095495, 9.69376718725741, 3.86470334076432, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1.10429619979071e-06, 0.00707866980709414, 0.0410258537896836, 0.00073, 0.0008, 0.231, 0.00312, 4.2, 1.54, 31, 34.5, 96, 1824, 19080, 91400, 259200, 11660, 72250, 9456, 8683000, 0.00037, 1.5, 60, 16710.8668579357, 1210447973.41758, 1176.51771383421, 13399.4988929792, 219.163272201039, 2145.53757620115, 75.780613083906, 687.733393044817, 108.737538099881, 257.320421720328, 41.8048570915313, 213.784076627629, 38.5925603059016, 122.858824370984, 26.2443004934205, 51.2832419758606, 18.5843866085014, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 4.24569278712376e-06, 2.37854804206103e-05, 0.00378939002366882, 0.0307987724936742, 0.139335535872744, 0.953030820957948, 0.4, 0.92, 1.2, 7, 23.4, 52, 252.6, 138, 720, 600, 1620, 1800, 19870, 4450000, 5760, 2402000, 2400, 180, 16461.5355449425, 186.901822762738, 680.822114852206, 56.600199553612, 253.497727795126, 31.336994213632, 90.1129438632996, 26.7700098632894, 68.7398632167827, 12.7013914920005, 46.9207467753537, 11.7851710681317, 26.6402894181648, 8.56881958036277, 14.0028254435265, 6.0824211609565, 11.3983671070136, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0.000538495972117378, 0.000350824173981321, 0.0613735468624823, 0.00531809715728672, 0.137127259301222, 1e-06, 5.7e-05, 5e-06, 0.8, 2.45, 93.6, 51.2, 211.2, 2.91, 24.5, 0.0012, 3480, 0.106, 10800, 0.005, 1200, 60, 143122.703786722, 11547562.9444862, 1859.98050182794, 76820.6463369602, 707.650385699432, 6779.17569020733, 594.010543753449, 4731.12803828734, 136.214167700972, 1460.51060237591, 113.74175819849, 740.520012795786, 86.7963740751367, 211.444111149912, 43.0342541904766, 137.087748108324, 26.0245538517665, 68.2875136557133, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 7.52259595498829e-05, 0.000398711691752281, 0.00676091506506484, 0.0283435970472886, 0.194326389435333, 2.12899285561419, 0.00015, 0.369, 0.632, 17.1, 31.1, 27, 6, 3.6, 6.2, 180, 2340, 14400, 18000, 36000, 36000, 75600, 37509.681611014, 291.829562691577, 3131.37212274751, 129.061104523401, 647.117560908799, 117.427529303441, 455.630071258339, 47.6269130755845, 279.906363916393, 33.5381667462721, 91.6719515639548, 21.5042496399534, 40.3621475088041, 12.0121923345514, 30.6817348496329, 8.86062402826592, 17.0137195601998, 6.4214923335082, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 6.64946892169303e-06, 2.61360598166868e-06, 0.00110120083982045, 0.00102430938238392, 0.188756310181151, 0.016141145684885, 0.013, 2.32e-05, 1.66, 0.00667, 4.82, 0.0138, 2.63, 0.021, 2.2, 0.25, 660, 3600, 96, 14400, 9000, 3600, 963564.990455408, 98713.6585667791, 14612.7496889776, 769.031664319342, 6302.73096363098, 86.1313042456948, 980.573946067952, 4400.9934328483, 941.014706451416, 17640.7425171, 378.695460164922, 1486.62397745544, 112.543033162503, 869.61521048939, 89.6700052132264, 244.743988919103, 42.915900235811, 126.5159119796, 28.2444523855969, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 2.28005806548793e-06, 0.00052466388728985, 0.00355349146359689, 0.0439249040847015, 0.214437587134952, 1.7, 1.7, 2.3, 4.5, 0.51, 1.52, 4.7, 34, 29, 180, 900, 4800, 6000, 104400, 10800, 7200, 118882.681462613, 1274.66145786317, 11372.9252548322, 622.219713889621, 516.57487878637, 198.159384758186, 3153.86971808149, 172.41526758767, 658.943817325466, 74.6335578098379, 162.243471969397, 27.0985133313147, 100.752025054174, 21.7973006905179, 48.2771977682647, 13.2965410491884, 24.149051606882, 8.60423570560011, 14.1308887996541, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0.000171003309269143, 0.000137879977210771, 0.0628767460651753, 0.0570570487624999, 2.23200191938366, 0.0027, 0.402, 0.00495, 0.183, 0.0109, 0.94, 0.047, 9.2, 0.39, 108, 120, 300, 180, 186, 240, 300, 231.030902054691, 3587.35801607277, 17.2878713824126, 1561.02308601709, 66.1634914790705, 23033.3319918063, 1824.00668429494, 5760.68587282464, 27052.4032263527, 482.044737041108, 8288.56135044038, 421.681491622798, 1672.09204703817, 173.186216214266, 526.883830768227, 77.2356030481622, 99.518604066804, 10.1254151866193, 0.00825363550568581, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 8.66730220259878e-05, 0.00050112031634634, 0.0340307006388079, 0.238590788092564, 0.117462258495203, 0.041, 0.0128, 0.084, 0.2, 1.07, 1.19, 2.5, 22, 25, 60, 228, 600, 11.3, 60, 60, 300, 1181.70006365536, 971.030354701119, 373.616150174293, 2034.9317267349, 1040.7612116728, 11414.9926125127, 398.698884449847, 1186.49828046819, 107.323693118238, 357.714096233606, 74.3699880487886, 149.527469435993, 36.1288549673881, 72.999759707581, 21.6409820185473, 37.8824795114836, 0.202536454913012, 2.92481972871363, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 1.33825667348371e-05, 2.15034977832905e-05, 0.00674300217614168, 0.000412468320157936, 0.00169692977402576, 0.0001167792359184, 0.00076, 0.00054, 0.00196, 0.00302, 0.055, 1.42, 16, 9, 10, 10, 1.06, 0.5, 0.29, 0.1, 0.011, 265.083562956176, 2612.458412161, 271.50033226817, 243557.300390872, 5872.16360911321, 2632992.81980014, 120806.593164054, 23361.7826061721, 1503795.87313017, 5191.66772306727, 61089.0137057233, 939.422456940495, 4217.3518226859, 278.90817236397, 815.193873312205, 57.8665047036431, 0.503301604322342, 0.139355718416768, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1.39949812329543e-05, 7.97791029396419e-05, 0.000217548801929972, 5.86661659621076e-05, 2.08072460206985e-05, 0.000220928286710015, 0.002, 0.0012, 0.01, 0.027, 0.1, 0.0063, 0.4, 0.4, 0.8, 0.85, 0.117, 0.63, 9, 7, 30, 433.1819542587, 4211.97100507592, 11943.3819409904, 270010.124663239, 5393.12035094966, 71411.0998297338, 662.154160812986, 9204.50532516936, 319.536513394257, 875.363742706083, 128.745052881842, 297.923691197959, 60.7675478026667, 123.179394049539, 18.573740528972, 25.2310745499017, 0.0446815461552107, 0.000692401075263218, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 3.3380269838425e-06, 1.51763927478293e-06, 1.00296658566369e-05, 1e-06, 2.6489566208931e-05, 3.90257134113273e-06, 1e-05, 0.0001, 0.00023, 0.000205, 0.09, 0.2, 0.00024, 0.01, 0.01, 0.1, 0.006, 0.27, 0.21, 11, 14, 388.149603439638, 1018947.56479063, 312913.604660461, 242713509.838962, 8785570.66640811, 13784784.4669256, 104269839.419087, 831522.898446572, 1237793076.5538, 12497.5849732981, 930474.972172753, 1642.35043331956, 11352.3949906324, 153.517992962473, 8.37889439357413, 0.556741762734804, 7.68495204928715e-05, 2.2843681140945e-05, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1e-06, 7.04472289933297e-06, 7.44811020173209e-06, 4.52655807548812e-05, 9.79206453501023e-05, 0.000928340107866936, 0.00268198545516458, 0.0045, 0.002, 0.029, 0.005, 0.01, 0.01, 0.008, 0.18, 4.3, 24, 96, 30, 679464.965994323, 11703306.9755503, 24505993.4901411, 10888500.3707635, 17904.6783517276, 969641895.47518, 2686.63441702131, 23360.9271773868, 653.717787677172, 2270.86100288598, 206.887928037435, 602.096968551892, 47.3484845158461, 166.434948046771, 0.166454266046506, 0.0270283881809974, 1.70555661519728e-05, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 2.13447747198594e-06, 1.51184131654273e-06, 0.000124254008411327, 3.54118281028902e-05, 0.000702137026785322, 0.000107658288460671, 0.000738909683684661, 0.0001, 0.00085, 0.002, 0.005, 0.005, 0.18, 0.0009, 4.1, 0.104, 32, 24027.6624434569, 545341.769073382, 77188.0436549433, 2999603.11056726, 43296522.7822782, 31072393372.6795, 170289437.788001, 1802553561.7002, 132880634.244033, 124219.439874032, 3611406.04945091, 1245.89878087217, 325.363868012208, 3.54723179183259, 0.113167822132302, 0.000623504567271667, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 5.30057066909008e-06, 3.17813527469946e-05, 0.000133987118272656, 9.25300797578276e-05, 0.000203889485442098, 2.56361343810535e-05, 0.0023, 0.001, 0.01, 0.1, 0.14, 0.16, 0.93, 3.3, 7, 120, 66326.013914109, 424900.831521571, 563841.754713405, 54105075.447302, 6383797.49590457, 23778194.5813162, 18313.9565805827, 1670144.52413358, 1887.59454654675, 16193.1945592619, 176.659896167218, 3080.78744591019, 1.78703676403831, 0.187158692795783, 0.00214002415152095, 3.94084381983e-05, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 1e-06, 1.96994934472114e-05, 3.27407277999478e-06, 4.14463215597207e-05, 1e-06, 0.000163294821967545, 0.000133356855237607, 0.0619470000861586, 1.61010434251134, 347.138065611302, 0.0033, 0.21, 0.14, 0.52, 0.75, 2.4, 120041.81680822, 2116342.18266851, 943259.611777175, 11957599.1341979, 44422.392920396, 439055.408557279, 24956.1311730766, 68863.5351109059, 3992.42509973508, 81.9262511606161, 45.3528433333664, 30.8706866603018, 0.0268394937027057, 0.000267342469714916, 4.34007314157164e-05, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 1e-06, 7.35636461450821e-06, 1.59738803153142e-05, 5.01971033978165e-05, 0.000420959231747098, 0.0017816514829433, 0.311514762996023, 12.6654080680132, 5.2403194940327, 2.7066083913243, 9.28661185061528, 0.095, 0.17, 0.31, 0.41, 1, 1101.78877102289, 787.431428569336, 9766.88869480718, 235.226115819884, 389.834383792276, 125.333436274352, 121.445579751596, 51.5564894779199, 0.477334700945679, 1.19405376949775, 0.494728436743839, 0.0673754008757905, 1.19990529854295e-05, 1.68465050472004e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1e-06, 5.05041843276106e-06, 7.44392820791539e-06, 0.000317096470150978, 0.000370638750285179, 0.00200674752616731, 0.000496284404830055, 0.0130288122346522, 0.00139241882731692, 0.0315214902153878, 0.00242300717099522, 0.002, 0.008, 0.028, 0.024, 0.08, 0.0167882949621332, 0.420520751633709, 0.00790853352721682, 0.0654665205036941, 0.00399830530138489, 0.0225424802640997, 0.00147290273890814, 0.00067779705786069, 8.34736520026704e-05, 0.0014948130033264, 9.55557480259129e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.87181106009239e-05, 0.000207222307737255, 5.9536239514931e-05, 0.000202858522359036, 0.000167306752062673, 0.000636907031561377, 0.000456856429863993, 0.00143241054010451, 0.000802243797106778, 0.00266591112350153, 0.002, 0.01, 0.021, 0.07, 0.00630916844018187, 0.0465744283918399, 0.00267399424723866, 0.00216696243899842, 0.000620058136528406, 0.000778796135584011, 0.000372190980052244, 4.17230354501998e-05, 2.90478122249325e-05, 2.4196312944077e-05, 3.8489924466635e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.02719258320853e-06, 1.47137837317838e-05, 1.38528243926224e-06, 3.5425500095803e-05, 3.04309864376616e-06, 6.21656110042044e-05, 4.22564496931533e-06, 0.000162360914295006, 3.54379054305618e-05, 0.001, 0.00115, 0.01, 5.28691355356899e-05, 0.00152699795975388, 1.77940812721266e-05, 0.000110231827883452, 6.46817336283576e-06, 4.47905437389396e-05, 3.50376487119388e-06, 1.18034974945478e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2.44010931692369e-06, 6.98965232697329e-06, 1.56840154546208e-06, 7.62046584773037e-06, 7.13251285163203e-06, 1.87907275255685e-05, 3.12693823089323e-05, 0.000100429159792273, 3.12693835209971e-05, 2.83690875552146e-05, 6.07628279120566e-05, 0.00042724960601622, 4.78521697843927e-05, 2.0579193124803e-05, 5.43070503465944e-06, 7.31417901298637e-06, 3.3194524729484e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 1e-06, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
     };

    public static string MapLabelText(int Z, int N, int E)
    {
        if (Z>=0 && Z <= MAXP && (Z+N) > 0) //use the known names
        {
            String ret = names[Z];
            return ret;
        }
        else if ((Z+N) > 0) //construct according to Z-A
        {
            String ret = Z.ToString();
            return ret;
        }
        else if (E > 0)
        {
            return "e-";
        }
        else
        {
            return "e+";
        }
    }
    public static string MainLabelText(int Z, int N, int E)
    {
        if (Z >= 0 && Z <= MAXP && (Z + N) > 0) //use the known names
        {
            String ret = names[Z];
            ret = ret + "-" + (Z + N).ToString();
            return ret;
        }
        else if ((Z + N) > 0) //construct according to Z-A
        {
            String ret = Z.ToString();
            ret = ret + "-" + (Z + N).ToString();
            return ret;
        }
        else if (E > 0)
        {
            return "e-";
        }
        else
        {
            return "e+";
        }
    }

    void Awake() {
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        
        decaytypes = new double[MAXP+1,MAXN+1][,];
        string descriptionText = System.IO.File.ReadAllText("Assets/Resources/scripts/Descriptions.txt");
        if (descriptionText != null)
        {
            descriptions = System.Text.RegularExpressions.Regex.Split(descriptionText, "------");
        }
        else
        {
            Debug.Log("Could not load descriptions");
        }


        decaytypes[0, 1] = new double[,] { { 0, 1.000 } };
        decaytypes[0, 2] = new double[,] { { 2, 1.000 } };
        decaytypes[1, 0] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[1, 1] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[1, 2] = new double[,] { { 0, 1.000 } };
        decaytypes[2, 0] = new double[,] { { 3, 1.000 } };
        decaytypes[2, 1] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[3, 0] = new double[,] { { 3, 1.000 } };
        decaytypes[1, 3] = new double[,] { { 2, 1.000 } };
        decaytypes[2, 2] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[3, 1] = new double[,] { { 3, 1.000 } };
        decaytypes[1, 4] = new double[,] { { 4, 1.000 } };
        decaytypes[2, 3] = new double[,] { { 2, 1.000 } };
        decaytypes[3, 2] = new double[,] { { 3, 1.000 } };
        decaytypes[4, 1] = new double[,] { { 3, 1.000 } };
        decaytypes[1, 5] = new double[,] { { 2, 0.500 }, { 6, 0.500 } };
        decaytypes[2, 4] = new double[,] { { 0, 1.000 } };
        decaytypes[3, 3] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[4, 2] = new double[,] { { 5, 1.000 } };
        decaytypes[5, 1] = new double[,] { { 5, 1.000 } };
        decaytypes[1, 6] = new double[,] { { 4, 1.000 } };
        decaytypes[2, 5] = new double[,] { { 2, 1.000 } };
        decaytypes[3, 4] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[4, 3] = new double[,] { { 18, 1.000 } };
        decaytypes[5, 2] = new double[,] { { 3, 1.000 } };
        decaytypes[2, 6] = new double[,] { { 0, 0.831 }, { 11, 0.160 }, { 17, 0.009 } };
        decaytypes[3, 5] = new double[,] { { 9, 1.000 } };
        decaytypes[4, 4] = new double[,] { { 8, 1.000 } };
        decaytypes[5, 3] = new double[,] { { 10, 1.000 } };
        decaytypes[6, 2] = new double[,] { { 5, 1.000 } };
        decaytypes[2, 7] = new double[,] { { 2, 1.000 } };
        decaytypes[3, 6] = new double[,] { { 0, 0.492 }, { 11, 0.508 } };
        decaytypes[4, 5] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[5, 4] = new double[,] { { 3, 1.000 } };
        decaytypes[6, 3] = new double[,] { { 10, 0.384 }, { 15, 0.616 } };
        decaytypes[2, 8] = new double[,] { { 4, 1.000 } };
        decaytypes[3, 7] = new double[,] { { 2, 1.000 } };
        decaytypes[4, 6] = new double[,] { { 0, 1.000 } };
        decaytypes[5, 5] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[6, 4] = new double[,] { { 1, 1.000 } };
        decaytypes[7, 3] = new double[,] { { 3, 1.000 } };
        decaytypes[3, 8] = new double[,] { { 0, 0.060 }, { 9, 0.017 }, { 11, 0.863 }, { 12, 0.041 }, { 13, 0.019 } };
        decaytypes[4, 7] = new double[,] { { 0, 0.971 }, { 9, 0.028 }, { 11, 0.001 } };
        decaytypes[5, 6] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[6, 5] = new double[,] { { 1, 1.000 } };
        decaytypes[7, 4] = new double[,] { { 3, 1.000 } };
        decaytypes[3, 9] = new double[,] { { 2, 1.000 } };
        decaytypes[4, 8] = new double[,] { { 0, 0.995 }, { 11, 0.005 } };
        decaytypes[5, 7] = new double[,] { { 0, 0.984 }, { 9, 0.016 } };
        decaytypes[6, 6] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[7, 5] = new double[,] { { 1, 0.965 }, { 10, 0.035 } };
        decaytypes[8, 4] = new double[,] { { 5, 1.000 } };
        decaytypes[3, 10] = new double[,] { { 4, 1.000 } };
        decaytypes[4, 9] = new double[,] { { 2, 1.000 } };
        decaytypes[5, 8] = new double[,] { { 0, 1.000 } };
        decaytypes[6, 7] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[7, 6] = new double[,] { { 1, 1.000 } };
        decaytypes[8, 5] = new double[,] { { 1, 0.891 }, { 15, 0.109 } };
        decaytypes[4, 10] = new double[,] { { 0, 0.012 }, { 11, 0.980 }, { 12, 0.008 } };
        decaytypes[5, 9] = new double[,] { { 0, 0.94 }, { 11, 0.06 } };
        decaytypes[6, 8] = new double[,] { { 0, 1.000 } };
        decaytypes[7, 7] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[8, 6] = new double[,] { { 1, 1.000 } };
        decaytypes[9, 5] = new double[,] { { 3, 1.000 } };
        decaytypes[4, 11] = new double[,] { { 2, 1.000 } };
        decaytypes[5, 10] = new double[,] { { 0, 0.064 }, { 11, 0.936 } };
        decaytypes[6, 9] = new double[,] { { 0, 1.000 } };
        decaytypes[7, 8] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[8, 7] = new double[,] { { 1, 1.000 } };
        decaytypes[9, 6] = new double[,] { { 3, 1.000 } };
        decaytypes[10, 5] = new double[,] { { 5, 1.000 } };
        decaytypes[4, 12] = new double[,] { { 4, 1.000 } };
        decaytypes[5, 11] = new double[,] { { 2, 1.000 } };
        decaytypes[6, 10] = new double[,] { { 0, 0.021 }, { 11, 0.979 } };
        decaytypes[7, 9] = new double[,] { { 0, 1.000 } };
        decaytypes[8, 8] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[9, 7] = new double[,] { { 3, 1.000 } };
        decaytypes[10, 6] = new double[,] { { 5, 1.000 } };
        decaytypes[5, 12] = new double[,] { { 0, 0.225 }, { 11, 0.630 }, { 12, 0.110 }, { 13, 0.035 } };
        decaytypes[6, 11] = new double[,] { { 0, 0.716 }, { 11, 0.284 } };
        decaytypes[7, 10] = new double[,] { { 0, 0.050 }, { 11, 0.950 } };
        decaytypes[8, 9] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[9, 8] = new double[,] { { 1, 1.000 } };
        decaytypes[10, 7] = new double[,] { { 1, 0.013 }, { 10, 0.027 }, { 15, 0.960 } };
        decaytypes[11, 6] = new double[,] { { 3, 1.000 } };
        decaytypes[5, 13] = new double[,] { { 2, 1.000 } };
        decaytypes[6, 12] = new double[,] { { 0, 0.685 }, { 11, 0.315 }};
        decaytypes[7, 11] = new double[,] { { 0, 0.808 }, { 9, 0.122 }, { 11, 0.070 } };
        decaytypes[8, 10] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[9, 9] = new double[,] { { 1, 1.000 } };
        decaytypes[10, 8] = new double[,] { { 1, 1.000 } };
        decaytypes[11, 7] = new double[,] { { 3, 1.000 } };
        decaytypes[5, 14] = new double[,] { { 0, 0.029 }, { 11, 0.710 }, { 12, 0.170 }, { 13, 0.091 } };
        decaytypes[6, 13] = new double[,] { { 0, 0.460 }, { 11, 0.470 }, { 12, 0.070 } };
        decaytypes[7, 12] = new double[,] { { 0, 0.582 }, { 11, 0.418 } };
        decaytypes[8, 11] = new double[,] { { 0, 1.000 } };
        decaytypes[9, 10] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[10, 9] = new double[,] { { 1, 1.000 } };
        decaytypes[11, 8] = new double[,] { { 3, 1.000 } };
        decaytypes[12, 7] = new double[,] { { 5, 1.000 } };
        decaytypes[5, 15] = new double[,] { { 2, 1.000 } };
        decaytypes[6, 14] = new double[,] { { 0, 0.114 }, { 11, 0.700 }, { 12, 0.186 } };
        decaytypes[7, 13] = new double[,] { { 0, 0.571 }, { 11, 0.429 } };
        decaytypes[8, 12] = new double[,] { { 0, 1.000 } };
        decaytypes[9, 11] = new double[,] { { 0, 1.000 } };
        decaytypes[10, 10] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[11, 9] = new double[,] { { 1, 0.750 }, { 10, 0.250 } };
        decaytypes[12, 8] = new double[,] { { 1, 0.697 }, { 15, 0.303 } };
        decaytypes[5, 16] = new double[,] { { 2, 1.000 } };
        decaytypes[6, 15] = new double[,] { { 2, 1.000 } };
        decaytypes[7, 14] = new double[,] { { 0, 0.095 }, { 11, 0.905 }};
        decaytypes[8, 13] = new double[,] { { 0, 1.000 } };
        decaytypes[9, 12] = new double[,] { { 0, 1.000 } };
        decaytypes[10, 11] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[11, 10] = new double[,] { { 1, 1.000 } };
        decaytypes[12, 9] = new double[,] { { 1, 0.674 }, { 15, 0.326 } };
        decaytypes[13, 8] = new double[,] { { 3, 1.000 } };
        decaytypes[6, 16] = new double[,] { { 0, 0.020 }, { 11, 0.610 }, { 12, 0.370 } };
        decaytypes[7, 15] = new double[,] { { 0, 0.540 }, { 11, 0.340 }, { 12, 0.120 } };
        decaytypes[8, 14] = new double[,] { { 0, 0.780 }, { 11, 0.220 } };
        decaytypes[9, 13] = new double[,] { { 0, 0.890 }, { 11, 0.110 } };
        decaytypes[10, 12] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[11, 11] = new double[,] { { 1, 1.000 } };
        decaytypes[12, 10] = new double[,] { { 1, 1.000 } };
        decaytypes[13, 9] = new double[,] { { 1, 0.439 }, { 15, 0.550 }, { 16, 0.011 } };
        decaytypes[14, 8] = new double[,] { { 1, 0.680 }, { 15, 0.320 } };
        decaytypes[6, 17] = new double[,] { { 2, 1.000 } };
        decaytypes[7, 16] = new double[,] { { 0, 0.466 }, { 11, 0.420 }, { 12, 0.080 }, { 13, 0.034 } };
        decaytypes[8, 15] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[9, 14] = new double[,] { { 0, 0.860 }, { 11, 0.140 } };
        decaytypes[10, 13] = new double[,] { { 0, 1.000 } };
        decaytypes[11, 12] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[12, 11] = new double[,] { { 1, 1.000 } };
        decaytypes[13, 10] = new double[,] { { 1, 1.000 } };
        decaytypes[14, 9] = new double[,] { { 1, 0.084 }, { 15, 0.880 }, { 16, 0.036 } };
        decaytypes[7, 17] = new double[,] { { 2, 1.000 } };
        decaytypes[8, 16] = new double[,] { { 0, 0.570 }, { 11, 0.430 } };
        decaytypes[9, 15] = new double[,] { { 0, 0.941 }, { 11, 0.059 } };
        decaytypes[10, 14] = new double[,] { { 0, 1.000 } };
        decaytypes[11, 13] = new double[,] { { 0, 1.000 } };
        decaytypes[12, 12] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[13, 11] = new double[,] { { 1, 1.000 } };
        decaytypes[14, 10] = new double[,] { { 1, 0.624 }, { 15, 0.376 } };
        decaytypes[15, 9] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[7, 18] = new double[,] { { 0, 0.333 }, { 2, 0.334 }, { 4, 0.333 } };
        decaytypes[8, 17] = new double[,] { { 2, 1.000 } };
        decaytypes[9, 16] = new double[,] { { 0, 0.769 }, { 11, 0.231 } };
        decaytypes[10, 15] = new double[,] { { 0, 1.000 } };
        decaytypes[11, 14] = new double[,] { { 0, 1.000 } };
        decaytypes[12, 13] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[13, 12] = new double[,] { { 1, 1.000 } };
        decaytypes[14, 11] = new double[,] { { 1, 0.650 }, { 15, 0.350 } };
        decaytypes[15, 10] = new double[,] { { 3, 1.000 } };
        decaytypes[8, 18] = new double[,] { { 4, 1.000 } };
        decaytypes[9, 17] = new double[,] { { 0, 0.865 }, { 11, 0.135 } };
        decaytypes[10, 16] = new double[,] { { 0, 1.000 } };
        decaytypes[11, 15] = new double[,] { { 0, 1.000 } };
        decaytypes[12, 14] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[13, 13] = new double[,] { { 1, 1.000 } };
        decaytypes[14, 12] = new double[,] { { 1, 1.000 } };
        decaytypes[15, 11] = new double[,] { { 1, 0.610 }, { 15, 0.368 }, { 16, 0.022 } };
        decaytypes[16, 10] = new double[,] { { 5, 1.000 } };
        decaytypes[8, 19] = new double[,] { { 2, 0.500 }, { 4, 0.500 } };
        decaytypes[9, 18] = new double[,] { { 0, 0.180 }, { 11, 0.770 }, { 12, 0.050 } };
        decaytypes[10, 17] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[11, 16] = new double[,] { { 0, 1.000 } };
        decaytypes[12, 15] = new double[,] { { 0, 1.000 } };
        decaytypes[13, 14] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[14, 13] = new double[,] { { 1, 1.000 } };
        decaytypes[15, 12] = new double[,] { { 1, 1.000 } };
        decaytypes[16, 11] = new double[,] { { 1, 0.966 }, { 15, 0.023 }, { 16, 0.011 } };
        decaytypes[8, 20] = new double[,] { { 2, 0.500 }, { 4, 0.500 } };
        decaytypes[9, 19] = new double[,] { { 2, 1.000 } };
        decaytypes[10, 18] = new double[,] { { 0, 0.843 }, { 11, 0.120 }, { 12, 0.037 } };
        decaytypes[11, 17] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[12, 16] = new double[,] { { 0, 1.000 } };
        decaytypes[13, 15] = new double[,] { { 0, 1.000 } };
        decaytypes[14, 14] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[15, 13] = new double[,] { { 1, 1.000 } };
        decaytypes[16, 12] = new double[,] { { 1, 0.793 }, { 15, 0.207 } };
        decaytypes[17, 11] = new double[,] { { 3, 1.000 } };
        decaytypes[9, 20] = new double[,] { { 0, 0.350 }, { 11, 0.600 }, { 12, 0.050 } };
        decaytypes[10, 19] = new double[,] { { 0, 0.680 }, { 11, 0.280 }, { 12, 0.040 } };
        decaytypes[11, 18] = new double[,] { { 0, 0.741 }, { 11, 0.259 } };
        decaytypes[12, 17] = new double[,] { { 0, 1.000 } };
        decaytypes[13, 16] = new double[,] { { 0, 1.000 } };
        decaytypes[14, 15] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[15, 14] = new double[,] { { 1, 1.000 } };
        decaytypes[16, 13] = new double[,] { { 1, 0.536 }, { 15, 0.464 } };
        decaytypes[17, 12] = new double[,] { { 3, 1.000 } };
        decaytypes[9, 21] = new double[,] { { 2, 1.000 } };
        decaytypes[10, 20] = new double[,] { { 0, 0.781 }, { 11, 0.130 }, { 12, 0.089 } };
        decaytypes[11, 19] = new double[,] { { 0, 0.688 }, { 11, 0.300 }, { 12, 0.012 } };
        decaytypes[12, 18] = new double[,] { { 0, 1.000 } };
        decaytypes[13, 17] = new double[,] { { 0, 1.000 } };
        decaytypes[14, 16] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[15, 15] = new double[,] { { 1, 1.000 } };
        decaytypes[16, 14] = new double[,] { { 1, 1.000 } };
        decaytypes[17, 13] = new double[,] { { 3, 1.000 } };
        decaytypes[18, 12] = new double[,] { { 5, 1.000 } };
        decaytypes[9, 22] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[10, 21] = new double[,] { { 0, 0.860 }, { 11, 0.100 }, { 12, 0.040 } };
        decaytypes[11, 20] = new double[,] { { 0, 0.618 }, { 11, 0.373 }, { 12, 0.009 } };
        decaytypes[12, 19] = new double[,] { { 0, 0.938 }, { 11, 0.062 } };
        decaytypes[13, 18] = new double[,] { { 0, 0.984 }, { 11, 0.016 } };
        decaytypes[14, 17] = new double[,] { { 0, 1.000 } };
        decaytypes[15, 16] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[16, 15] = new double[,] { { 1, 1.000 } };
        decaytypes[17, 14] = new double[,] { { 1, 0.976 }, { 15, 0.024 } };
        decaytypes[18, 13] = new double[,] { { 1, 0.227 }, { 15, 0.683 }, { 16, 0.090 } };
        decaytypes[10, 22] = new double[,] { { 0, 0.630 }, { 11, 0.300 }, { 12, 0.070 } };
        decaytypes[11, 21] = new double[,] { { 0, 0.680 }, { 11, 0.240 }, { 12, 0.080 } };
        decaytypes[12, 20] = new double[,] { { 0, 0.945 }, { 11, 0.055 } };
        decaytypes[13, 19] = new double[,] { { 0, 0.993 }, { 11, 0.007 } };
        decaytypes[14, 18] = new double[,] { { 0, 1.000 } };
        decaytypes[15, 17] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 16] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[17, 15] = new double[,] { { 1, 1.000 } };
        decaytypes[18, 14] = new double[,] { { 1, 0.644 }, { 15, 0.356 } };
        decaytypes[19, 13] = new double[,] { { 3, 1.000 } };
        decaytypes[10, 23] = new double[,] { { 2, 1.000 } };
        decaytypes[11, 22] = new double[,] { { 0, 0.400 }, { 11, 0.470 }, { 12, 0.130 } };
        decaytypes[12, 21] = new double[,] { { 0, 0.830 }, { 11, 0.140 }, { 12, 0.030 } };
        decaytypes[13, 20] = new double[,] { { 0, 0.915 }, { 11, 0.085 } };
        decaytypes[14, 19] = new double[,] { { 0, 1.000 } };
        decaytypes[15, 18] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 17] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[17, 16] = new double[,] { { 1, 1.000 } };
        decaytypes[18, 15] = new double[,] { { 1, 0.613 }, { 15, 0.387 } };
        decaytypes[19, 14] = new double[,] { { 3, 1.000 } };
        decaytypes[10, 24] = new double[,] { { 0, 0.590 }, { 11, 0.010 }, { 12, 0.400 } };
        decaytypes[11, 23] = new double[,] { { 0, 0.350 }, { 11, 0.150 }, { 12, 0.500 } };
        decaytypes[12, 22] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[13, 21] = new double[,] { { 0, 0.740 }, { 11, 0.260 } };
        decaytypes[14, 20] = new double[,] { { 0, 1.000 } };
        decaytypes[15, 19] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 18] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[17, 17] = new double[,] { { 1, 1.000 } };
        decaytypes[18, 16] = new double[,] { { 1, 1.000 } };
        decaytypes[19, 15] = new double[,] { { 3, 1.000 } };
        decaytypes[20, 14] = new double[,] { { 5, 1.000 } };
        decaytypes[11, 24] = new double[,] { { 0, 0.3 }, { 11, 0.6 }, { 12, 0.1 } };
        decaytypes[12, 23] = new double[,] { { 0, 0.280 }, { 11, 0.520 }, { 12, 0.200 } };
        decaytypes[13, 22] = new double[,] { { 0, 0.620 }, { 11, 0.380 } };
        decaytypes[14, 21] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[15, 20] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 19] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 18] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[18, 17] = new double[,] { { 1, 1.000 } };
        decaytypes[19, 16] = new double[,] { { 1, 1.000 } };
        decaytypes[20, 15] = new double[,] { { 15, 0.959 }, { 16, 0.041 } };
        decaytypes[11, 25] = new double[,] { { 2, 1.000 } };
        decaytypes[12, 24] = new double[,] { { 0, 0.670 }, { 11, 0.300 }, { 12, 0.030 } };
        decaytypes[13, 23] = new double[,] { { 0, 0.630 }, { 11, 0.300 }, { 12, 0.070 } };
        decaytypes[14, 22] = new double[,] { { 0, 0.880 }, { 11, 0.120 } };
        decaytypes[15, 21] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 20] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[17, 19] = new double[,] { { 0, 0.981 }, { 1, 0.019 } };
        decaytypes[18, 18] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[19, 17] = new double[,] { { 1, 1.000 } };
        decaytypes[20, 16] = new double[,] { { 1, 0.488 }, { 15, 0.512 } };
        decaytypes[21, 15] = new double[,] { { 3, 1.000 } };
        decaytypes[11, 26] = new double[,] { { 11, 1.000 } };
        decaytypes[12, 25] = new double[,] { { 11, 0.800 }, { 12, 0.200 } };
        decaytypes[13, 24] = new double[,] { { 0, 0.700 }, { 11, 0.290 }, { 12, 0.010 } };
        decaytypes[14, 23] = new double[,] { { 0, 0.830 }, { 11, 0.170 } };
        decaytypes[15, 22] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 21] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 20] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[18, 19] = new double[,] { { 18, 1.000 } };
        decaytypes[19, 18] = new double[,] { { 1, 1.000 } };
        decaytypes[20, 17] = new double[,] { { 1, 0.179 }, { 15, 0.821 } };
        decaytypes[21, 16] = new double[,] { { 3, 1.000 } };
        decaytypes[12, 26] = new double[,] { { 0, 0.130 }, { 11, 0.800 }, { 12, 0.070 } };
        decaytypes[13, 25] = new double[,] { { 0, 0.900 }, { 12, 0.100 } };
        decaytypes[14, 24] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[15, 23] = new double[,] { { 0, 0.880 }, { 11, 0.120 } };
        decaytypes[16, 22] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 21] = new double[,] { { 0, 1.000 } };
        decaytypes[18, 20] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[19, 19] = new double[,] { { 1, 1.000 } };
        decaytypes[20, 18] = new double[,] { { 1, 1.000 } };
        decaytypes[21, 17] = new double[,] { { 3, 1.000 } };
        decaytypes[22, 16] = new double[,] { { 5, 1.000 } };
        decaytypes[12, 27] = new double[,] { { 2, 1.000 } };
        decaytypes[13, 26] = new double[,] { { 0, 0.090 }, { 11, 0.900 }, { 12, 0.010 } };
        decaytypes[14, 25] = new double[,] { { 0, 0.730 }, { 11, 0.250 }, { 12, 0.020 } };
        decaytypes[15, 24] = new double[,] { { 0, 0.740 }, { 11, 0.260 } };
        decaytypes[16, 23] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 22] = new double[,] { { 0, 1.000 } };
        decaytypes[18, 21] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 20] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[20, 19] = new double[,] { { 1, 1.000 } };
        decaytypes[21, 18] = new double[,] { { 3, 1.000 } };
        decaytypes[22, 17] = new double[,] { { 1, 0.06 }, { 15, 0.8 }, { 16, 0.14 } };
        decaytypes[12, 28] = new double[,] { { 11, 0.5 }, { 12, 0.5 } };
        decaytypes[13, 27] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[14, 26] = new double[,] { { 11, 0.400 }, { 12, 0.600 } };
        decaytypes[15, 25] = new double[,] { { 0, 0.822 }, { 11, 0.158 }, { 12, 0.020 } };
        decaytypes[16, 24] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 23] = new double[,] { { 0, 1.000 } };
        decaytypes[18, 22] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[19, 21] = new double[,] { { 0, 0.893 }, { 1, 0.107 } };
        decaytypes[20, 20] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 19] = new double[,] { { 1, 1.000 } };
        decaytypes[22, 18] = new double[,] { { 1, 0.042 }, { 15, 0.958 } };
        decaytypes[23, 17] = new double[,] { { 3, 1.000 } };
        decaytypes[13, 28] = new double[,] { { 0, 0.400 }, { 11, 0.500 }, { 12, 0.100 } };
        decaytypes[14, 27] = new double[,] { { 0, 0.450 }, { 11, 0.450 }, { 12, 0.100 } };
        decaytypes[15, 26] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[16, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[17, 24] = new double[,] { { 0, 1.000 } };
        decaytypes[18, 23] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 22] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[20, 21] = new double[,] { { 18, 1.000 } };
        decaytypes[21, 20] = new double[,] { { 1, 1.000 } };
        decaytypes[22, 19] = new double[,] { { 1, 0.089 }, { 15, 0.911 } };
        decaytypes[23, 18] = new double[,] { { 3, 1.000 } };
        decaytypes[13, 29] = new double[,] { { 0, 0.300 }, { 11, 0.300 }, { 12, 0.400 } };
        decaytypes[14, 28] = new double[,] { { 11, 0.400 }, { 12, 0.600 } };
        decaytypes[15, 27] = new double[,] { { 0, 0.300 }, { 11, 0.500 }, { 12, 0.200 } };
        decaytypes[16, 26] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[17, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[18, 24] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 23] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 22] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 21] = new double[,] { { 1, 1.000 } };
        decaytypes[22, 20] = new double[,] { { 1, 1.000 } };
        decaytypes[23, 19] = new double[,] { { 3, 1.000 } };
        decaytypes[24, 18] = new double[,] { { 1, 0.028 }, { 5, 0.500 }, { 15, 0.472 } };
        decaytypes[13, 30] = new double[,] { { 11, 0.500 }, { 12, 0.500 } };
        decaytypes[14, 29] = new double[,] { { 0, 0.300 }, { 11, 0.400 }, { 12, 0.300 } };
        decaytypes[15, 28] = new double[,] { { 11, 0.900 }, { 12, 0.100 } };
        decaytypes[16, 27] = new double[,] { { 0, 0.600 }, { 11, 0.400 } };
        decaytypes[17, 26] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[18, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 24] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 23] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 22] = new double[,] { { 1, 1.000 } };
        decaytypes[22, 21] = new double[,] { { 15, 1.000 } };
        decaytypes[23, 20] = new double[,] { { 1, 0.975 }, { 15, 0.025 } };
        decaytypes[24, 19] = new double[,] { { 1, 0.2 }, { 10, 0.7 }, { 15, 0.05 }, { 16, 0.05 } };
        decaytypes[14, 30] = new double[,] { { 11, 0.500 }, { 12, 0.500 } };
        decaytypes[15, 29] = new double[,] { { 0, 0.100 }, { 11, 0.200 }, { 12, 0.700 } };
        decaytypes[16, 28] = new double[,] { { 0, 0.820 }, { 11, 0.180 } };
        decaytypes[17, 27] = new double[,] { { 0, 0.920 }, { 11, 0.080 } };
        decaytypes[18, 26] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 24] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 23] = new double[,] { { 1, 1.000 } };
        decaytypes[22, 22] = new double[,] { { 18, 1.000 } };
        decaytypes[23, 21] = new double[,] { { 1 , 0.9}, { 10, 0.09 }, { 15, 0.01 } };
        decaytypes[24, 20] = new double[,] { { 1, 0.860 }, { 15, 0.140 } };
        decaytypes[25, 19] = new double[,] { { 3, 1.000 } };
        decaytypes[14, 31] = new double[,] { { 11, 0.500 }, { 12, 0.500 } };
        decaytypes[15, 30] = new double[,] { { 0, 0.400 }, { 11, 0.300 }, { 12, 0.300 } };
        decaytypes[16, 29] = new double[,] { { 0, 0.420 }, { 11, 0.540 }, { 12, 0.040 } };
        decaytypes[17, 28] = new double[,] { { 0, 0.760 }, { 11, 0.240 } };
        decaytypes[18, 27] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 26] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[21, 24] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[22, 23] = new double[,] { { 1, 1.000 } };
        decaytypes[23, 22] = new double[,] { { 1, 1.000 } };
        decaytypes[24, 21] = new double[,] { { 1, 0.656 }, { 15, 0.344 } };
        decaytypes[25, 20] = new double[,] { { 3, 1.000 } };
        decaytypes[26, 19] = new double[,] { { 1, 0.180 }, { 5, 0.570 }, { 15, 0.250 } };
        decaytypes[15, 31] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[16, 30] = new double[,] { { 0, 0.270 }, { 11, 0.700 }, { 12, 0.030 } };
        decaytypes[17, 29] = new double[,] { { 0, 0.400 }, { 11, 0.600 } };
        decaytypes[18, 28] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 27] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 26] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 25] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 24] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[23, 23] = new double[,] { { 1, 1.000 } };
        decaytypes[24, 22] = new double[,] { { 1, 1.000 } };
        decaytypes[25, 21] = new double[,] { { 1, 0.43 }, { 10, 0.45 }, { 15, 0.1 }, { 16, 0.02 } };
        decaytypes[26, 20] = new double[,] { { 1, 0.213 }, { 15, 0.777 }, { 16, 0.01 } };
        decaytypes[15, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[16, 31] = new double[,] { { 0, 0.800 }, { 11, 0.100 }, { 12, 0.100 } };
        decaytypes[17, 30] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[18, 29] = new double[,] { { 0, 1.000 } };
        decaytypes[19, 28] = new double[,] { { 0, 1.000 } };
        decaytypes[20, 27] = new double[,] { { 0, 1.000 } };
        decaytypes[21, 26] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 25] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[23, 24] = new double[,] { { 1, 1.000 } };
        decaytypes[24, 23] = new double[,] { { 1, 1.000 } };
        decaytypes[25, 22] = new double[,] { { 1, 0.983 }, { 15, 0.017 } };
        decaytypes[26, 21] = new double[,] { { 1, 0.116 }, { 15, 0.884 } };
        decaytypes[27, 20] = new double[,] { { 3, 1.000 } };
        decaytypes[16, 32] = new double[,] { { 0, 0.100 }, { 11, 0.800 }, { 12, 0.100 } };
        decaytypes[17, 31] = new double[,] { { 11, 0.600 }, { 12, 0.400 } };
        decaytypes[18, 30] = new double[,] { { 0, 0.620 }, { 11, 0.380 } };
        decaytypes[19, 29] = new double[,] { { 0, 0.989 }, { 11, 0.011 } };
        decaytypes[20, 28] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[21, 27] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 26] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[23, 25] = new double[,] { { 1, 1.000 } };
        decaytypes[24, 24] = new double[,] { { 1, 1.000 } };
        decaytypes[25, 23] = new double[,] { { 1, 1.000 } };
        decaytypes[26, 22] = new double[,] { { 1, 0.847 }, { 15, 0.153 } };
        decaytypes[27, 21] = new double[,] { { 3, 1.000 } };
        decaytypes[28, 20] = new double[,] { { 5, 0.700 }, { 1, 0.25 }, { 15, 0.05 } };
        decaytypes[16, 33] = new double[,] { { 0, 0.330 }, { 2, 0.500 }, { 11, 0.020 }, { 12, 0.150 } };
        decaytypes[17, 32] = new double[,] { { 0, 0.100 }, { 11, 0.700 }, { 12, 0.200 } };
        decaytypes[18, 31] = new double[,] { { 0, 0.710 }, { 11, 0.290 } };
        decaytypes[19, 30] = new double[,] { { 0, 0.140 }, { 11, 0.860 } };
        decaytypes[20, 29] = new double[,] { { 0, 1.000 } };
        decaytypes[21, 28] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 27] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[23, 26] = new double[,] { { 18, 1.000 } };
        decaytypes[24, 25] = new double[,] { { 1, 1.000 } };
        decaytypes[25, 24] = new double[,] { { 1, 1.000 } };
        decaytypes[26, 23] = new double[,] { { 1, 0.433 }, { 15, 0.567 } };
        decaytypes[27, 22] = new double[,] { { 3, 1.000 } };
        decaytypes[28, 21] = new double[,] { { 1, 0.170 }, { 15, 0.830 } };
        decaytypes[17, 33] = new double[,] { { 11, 0.700 }, { 12, 0.300 } };
        decaytypes[18, 32] = new double[,] { { 0, 0.630 }, { 11, 0.370 } };
        decaytypes[19, 31] = new double[,] { { 0, 0.61 }, { 11, 0.29 }, { 12, 0.1 } };
        decaytypes[20, 30] = new double[,] { { 0, 1.000 } };
        decaytypes[21, 29] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 28] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[23, 27] = new double[,] { { 0, 0.170 }, { 1, 0.830 } };
        decaytypes[24, 26] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[25, 25] = new double[,] { { 1, 1.000 } };
        decaytypes[26, 24] = new double[,] { { 1, 1.000 } };
        decaytypes[27, 23] = new double[,] { { 1, 0.3 }, { 15, 0.695 }, { 16, 0.005} };
        decaytypes[28, 22] = new double[,] { { 1, 0.133 }, { 15, 0.857 }, { 16, 0.01 } };
        decaytypes[17, 34] = new double[,] { { 0, 0.400 }, { 11, 0.400 }, { 12, 0.200 } };
        decaytypes[18, 33] = new double[,] { { 0, 0.500 }, { 11, 0.400 }, { 12, 0.100 } };
        decaytypes[19, 32] = new double[,] { { 0, 0.310 }, { 11, 0.650 }, { 12, 0.040 } };
        decaytypes[20, 31] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[21, 30] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 29] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 28] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[24, 27] = new double[,] { { 18, 1.000 } };
        decaytypes[25, 26] = new double[,] { { 1, 1.000 } };
        decaytypes[26, 25] = new double[,] { { 1, 1.000 } };
        decaytypes[27, 24] = new double[,] { { 1, 0.962 }, { 15, 0.038 } };
        decaytypes[28, 23] = new double[,] { { 1, 0.123 }, { 15, 0.872 }, { 16, 0.005 } };
        decaytypes[18, 34] = new double[,] { { 0, 0.630 }, { 11, 0.300 }, { 12, 0.070 } };
        decaytypes[19, 33] = new double[,] { { 0, 0.237 }, { 11, 0.740 }, { 12, 0.023 } };
        decaytypes[20, 32] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[21, 31] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[22, 30] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 29] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 28] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[25, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[26, 26] = new double[,] { { 1, 1.000 } };
        decaytypes[27, 25] = new double[,] { { 15, 1.000 } };
        decaytypes[28, 24] = new double[,] { { 1, 0.689 }, { 15, 0.311 } };
        decaytypes[29, 23] = new double[,] { { 3, 1.000 } };
        decaytypes[18, 35] = new double[,] { { 0, 0.500 }, { 11, 0.200 }, { 12, 0.300 } };
        decaytypes[19, 34] = new double[,] { { 0, 0.260 }, { 11, 0.640 }, { 12, 0.100 } };
        decaytypes[20, 33] = new double[,] { { 0, 0.600 }, { 11, 0.400 } };
        decaytypes[21, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[22, 31] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 30] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 29] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[25, 28] = new double[,] { { 18, 1.000 } };
        decaytypes[26, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[27, 26] = new double[,] { { 1, 1.000 } };
        decaytypes[28, 25] = new double[,] { { 1, 0.773 }, { 15, 0.227 } };
        decaytypes[29, 24] = new double[,] { { 3, 1.000 } };
        decaytypes[19, 35] = new double[,] { { 0, 0.690 }, { 11, 0.010 }, { 12, 0.300 } };
        decaytypes[20, 34] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[21, 33] = new double[,] { { 0, 0.840 }, { 11, 0.160 } };
        decaytypes[22, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 31] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 30] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[25, 29] = new double[,] { { 18, 1.000 } };
        decaytypes[26, 28] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[27, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[28, 26] = new double[,] { { 15, 1.000 } };
        decaytypes[29, 25] = new double[,] { { 3, 1.000 } };
        decaytypes[30, 24] = new double[,] { { 5, 1.000 } };
        decaytypes[19, 36] = new double[,] { { 0, 0.590 }, { 11, 0.400 }, { 12, 0.010 } };
        decaytypes[20, 35] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[21, 34] = new double[,] { { 0, 0.830 }, { 11, 0.170 } };
        decaytypes[22, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 31] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 30] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[26, 29] = new double[,] { { 18, 1.000 } };
        decaytypes[27, 28] = new double[,] { { 1, 1.000 } };
        decaytypes[28, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[29, 26] = new double[,] { { 1, 0.850 }, { 15, 0.150 } };
        decaytypes[30, 25] = new double[,] { { 1, 0.090 }, { 15, 0.910 } };
        decaytypes[19, 37] = new double[,] { { 0, 0.100 }, { 11, 0.500 }, { 12, 0.400 } };
        decaytypes[20, 36] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[21, 35] = new double[,] { { 0, 0.895 }, { 11, 0.100 }, { 12, 0.005 } };
        decaytypes[22, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 31] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 30] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[27, 29] = new double[,] { { 1, 1.000 } };
        decaytypes[28, 28] = new double[,] { { 1, 1.000 } };
        decaytypes[29, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 26] = new double[,] { { 1, 0.120 }, { 15, 0.880 } };
        decaytypes[31, 25] = new double[,] { { 3, 1.000 } };
        decaytypes[20, 37] = new double[,] { { 0, 0.780 }, { 11, 0.200 }, { 12, 0.020 } };
        decaytypes[21, 36] = new double[,] { { 0, 0.690 }, { 11, 0.300 }, { 12, 0.010 } };
        decaytypes[22, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[24, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 32] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 31] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[27, 30] = new double[,] { { 18, 1.000 } };
        decaytypes[28, 29] = new double[,] { { 1, 1.000 } };
        decaytypes[29, 28] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 27] = new double[,] { { 1, 0.350 }, { 15, 0.650 } };
        decaytypes[31, 26] = new double[,] { { 3, 1.000 } };
        decaytypes[20, 38] = new double[,] { { 0, 0.940 }, { 11, 0.020 }, { 12, 0.040 } };
        decaytypes[21, 37] = new double[,] { { 0, 0.790 }, { 11, 0.200 }, { 12, 0.010 } };
        decaytypes[22, 36] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[23, 35] = new double[,] { { 0, 0.992 }, { 11, 0.008 } };
        decaytypes[24, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 32] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[27, 31] = new double[,] { { 1, 1.000 } };
        decaytypes[28, 30] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[29, 29] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 28] = new double[,] { { 1, 0.970 }, { 15, 0.030 } };
        decaytypes[31, 27] = new double[,] { { 3, 1.000 } };
        decaytypes[32, 26] = new double[,] { { 5, 1.000 } };
        decaytypes[21, 38] = new double[,] { { 0, 0.490 }, { 11, 0.500 }, { 12, 0.010 } };
        decaytypes[22, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[23, 36] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[24, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 32] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[28, 31] = new double[,] { { 1, 1.000 } };
        decaytypes[29, 30] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 29] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 28] = new double[,] { { 3, 1.000 } };
        decaytypes[32, 27] = new double[,] { { 1, 1.000 } };
        decaytypes[21, 39] = new double[,] { { 0, 0.500 }, { 12, 0.500 } };
        decaytypes[22, 38] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[23, 37] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[24, 36] = new double[,] { { 0, 1.000 } };
        decaytypes[25, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 33] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 32] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[29, 31] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 30] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 29] = new double[,] { { 1, 0.984 }, { 15, 0.016 } };
        decaytypes[32, 28] = new double[,] { { 15, 1.000 } };
        decaytypes[33, 27] = new double[,] { { 3, 1.000 } };
        decaytypes[21, 40] = new double[,] { { 0, 0.390 }, { 11, 0.600 }, { 12, 0.010 } };
        decaytypes[22, 39] = new double[,] { { 0, 0.980 }, { 11, 0.010 }, { 12, 0.010 } };
        decaytypes[23, 38] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[24, 37] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[25, 36] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[26, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 34] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 33] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[29, 32] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 31] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 30] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 29] = new double[,] { { 1, 0.380 }, { 15, 0.620 } };
        decaytypes[33, 28] = new double[,] { { 3, 1.000 } };
        decaytypes[22, 40] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[23, 39] = new double[,] { { 0, 0.795 }, { 11, 0.200 }, { 12, 0.005 } };
        decaytypes[24, 38] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[25, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 36] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 34] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[29, 33] = new double[,] { { 1, 1.000 } };
        decaytypes[30, 32] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 31] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 30] = new double[,] { { 15, 1.000 } };
        decaytypes[33, 29] = new double[,] { { 3, 1.000 } };
        decaytypes[22, 41] = new double[,] { { 0, 0.890 }, { 11, 0.070 }, { 12, 0.040 } };
        decaytypes[23, 40] = new double[,] { { 0, 0.650 }, { 11, 0.350 } };
        decaytypes[24, 39] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[25, 38] = new double[,] { { 0, 1.000 } };
        decaytypes[26, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 36] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 35] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 34] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[30, 33] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 32] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 31] = new double[,] { { 15, 1.000 } };
        decaytypes[33, 30] = new double[,] { { 3, 1.000 } };
        decaytypes[22, 42] = new double[,] { { 0, 0.080 }, { 11, 0.900 }, { 12, 0.020 } };
        decaytypes[23, 41] = new double[,] { { 0, 0.660 }, { 11, 0.300 }, { 12, 0.040 } };
        decaytypes[24, 40] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[25, 39] = new double[,] { { 0, 0.670 }, { 11, 0.330 } };
        decaytypes[26, 38] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 36] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[29, 35] = new double[,] { { 0, 0.385 }, { 1, 0.615 } };
        decaytypes[30, 34] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[31, 33] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 32] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 31] = new double[,] { { 15, 1.000 } };
        decaytypes[34, 30] = new double[,] { { 15, 1.000 } };
        decaytypes[23, 42] = new double[,] { { 0, 0.590 }, { 11, 0.400 }, { 12, 0.010 } };
        decaytypes[24, 41] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[25, 40] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[26, 39] = new double[,] { { 0, 0.921 }, { 11, 0.079 } };
        decaytypes[27, 38] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 36] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[30, 35] = new double[,] { { 1, 1.000 } };
        decaytypes[31, 34] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 33] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 32] = new double[,] { { 15, 1.000 } };
        decaytypes[34, 31] = new double[,] { { 15, 1.000 } };
        decaytypes[23, 43] = new double[,] { { 0, 0.400 }, { 11, 0.200 }, { 12, 0.400 } };
        decaytypes[24, 42] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[25, 41] = new double[,] { { 0, 0.916 }, { 11, 0.084 } };
        decaytypes[26, 40] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 39] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 38] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 37] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 36] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[31, 35] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 34] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 33] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 32] = new double[,] { { 15, 1.000 } };
        decaytypes[23, 44] = new double[,] { { 0, 0.370 }, { 11, 0.600 }, { 12, 0.030 } };
        decaytypes[24, 43] = new double[,] { { 0, 0.890 }, { 11, 0.100 }, { 12, 0.010 } };
        decaytypes[25, 42] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[26, 41] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[27, 40] = new double[,] { { 0, 1.000 } };
        decaytypes[28, 39] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 38] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 37] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[31, 36] = new double[,] { { 18, 1.000 } };
        decaytypes[32, 35] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 34] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 33] = new double[,] { { 1, 0.995 }, { 15, 0.005 } };
        decaytypes[35, 32] = new double[,] { { 3, 1.000 } };
        decaytypes[24, 44] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[25, 43] = new double[,] { { 0, 0.880 }, { 11, 0.100 }, { 12, 0.020 } };
        decaytypes[26, 42] = new double[,] { { 0, 1.000 } };
        decaytypes[27, 41] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[28, 40] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 39] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 38] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[31, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 36] = new double[,] { { 18, 1.000 } };
        decaytypes[33, 35] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 34] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 33] = new double[,] { { 3, 1.000 } };
        decaytypes[24, 45] = new double[,] { { 0, 0.740 }, { 11, 0.200 }, { 12, 0.060 } };
        decaytypes[25, 44] = new double[,] { { 0, 0.500 }, { 11, 0.500 } };
        decaytypes[26, 43] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[27, 42] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[28, 41] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 40] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 39] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 38] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[32, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 36] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 35] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 34] = new double[,] { { 3, 1.000 } };
        decaytypes[36, 33] = new double[,] { { 1, 0.450 }, { 15, 0.550 } };
        decaytypes[24, 46] = new double[,] { { 0, 0.580 }, { 11, 0.400 }, { 12, 0.020 } };
        decaytypes[25, 45] = new double[,] { { 0, 0.730 }, { 11, 0.200 }, { 12, 0.070 } };
        decaytypes[26, 44] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[27, 43] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[28, 42] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 41] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 40] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[31, 39] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 38] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[33, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 36] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 35] = new double[,] { { 15, 1.000 } };
        decaytypes[36, 34] = new double[,] { { 1, 0.987 }, { 15, 0.013 } };
        decaytypes[25, 46] = new double[,] { { 0, 0.670 }, { 11, 0.300 }, { 12, 0.030 } };
        decaytypes[26, 45] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[27, 44] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[28, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 42] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 41] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 40] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[32, 39] = new double[,] { { 18, 1.000 } };
        decaytypes[33, 38] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 36] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 35] = new double[,] { { 1, 0.979 }, { 15, 0.021 } };
        decaytypes[37, 34] = new double[,] { { 3, 1.000 } };
        decaytypes[25, 47] = new double[,] { { 0, 0.400 }, { 11, 0.500 }, { 12, 0.100 } };
        decaytypes[26, 46] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[27, 45] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[28, 44] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 42] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 41] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 40] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[33, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 38] = new double[,] { { 18, 1.000 } };
        decaytypes[35, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 36] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 35] = new double[,] { { 3, 1.000 } };
        decaytypes[26, 47] = new double[,] { { 0, 0.760 }, { 11, 0.200 }, { 12, 0.040 } };
        decaytypes[27, 46] = new double[,] { { 0, 0.910 }, { 11, 0.090 } };
        decaytypes[28, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[29, 44] = new double[,] { { 0, 1.000 } };
        decaytypes[30, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 42] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 41] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[33, 40] = new double[,] { { 18, 1.000 } };
        decaytypes[34, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 38] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 37] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 36] = new double[,] { { 3, 1.000 } };
        decaytypes[38, 35] = new double[,] { { 15, 1.000 } };
        decaytypes[26, 48] = new double[,] { { 0, 0.680 }, { 11, 0.300 }, { 12, 0.020 } };
        decaytypes[27, 47] = new double[,] { { 0, 0.730 }, { 11, 0.260 }, { 12, 0.010 } };
        decaytypes[28, 46] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[29, 45] = new double[,] { { 0, 0.600 }, { 11, 0.400 } };
        decaytypes[30, 44] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 42] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[33, 41] = new double[,] { { 0, 0.340 }, { 1, 0.660 } };
        decaytypes[34, 40] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 38] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 37] = new double[,] { { 15, 1.000 } };
        decaytypes[38, 36] = new double[,] { { 15, 1.000 } };
        decaytypes[26, 49] = new double[,] { { 11, 0.800 }, { 12, 0.200 } };
        decaytypes[27, 48] = new double[,] { { 0, 0.835 }, { 11, 0.160 }, { 12, 0.005 } };
        decaytypes[28, 47] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[29, 46] = new double[,] { { 0, 0.965 }, { 11, 0.035 } };
        decaytypes[30, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 44] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 42] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[34, 41] = new double[,] { { 18, 1.000 } };
        decaytypes[35, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 38] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 37] = new double[,] { { 1, 0.948 }, { 15, 0.052 } };
        decaytypes[39, 36] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[27, 49] = new double[,] { { 0, 0.900 }, { 11, 0.040 }, { 12, 0.060 } };
        decaytypes[28, 48] = new double[,] { { 0, 0.860 }, { 11, 0.140 } };
        decaytypes[29, 47] = new double[,] { { 0, 0.928 }, { 11, 0.072 } };
        decaytypes[30, 46] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 44] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[33, 43] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 42] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 38] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 37] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[27, 50] = new double[,] { { 0, 0.050 }, { 11, 0.900 }, { 12, 0.050 } };
        decaytypes[28, 49] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[29, 48] = new double[,] { { 0, 0.697 }, { 11, 0.303 } };
        decaytypes[30, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 46] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 44] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 43] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 39] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 38] = new double[,] { { 3, 0.091 }, { 15, 0.909 } };
        decaytypes[40, 37] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[28, 50] = new double[,] { { 0, 0.500 }, { 11, 0.500 } };
        decaytypes[29, 49] = new double[,] { { 0, 0.494 }, { 11, 0.506 } };
        decaytypes[30, 48] = new double[,] { { 0, 1.000 } };
        decaytypes[31, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 46] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 44] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[36, 42] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 39] = new double[,] { { 15, 1.000 } };
        decaytypes[40, 38] = new double[,] { { 15, 1.000 } };
        decaytypes[28, 51] = new double[,] { { 11, 0.600 }, { 12, 0.400 } };
        decaytypes[29, 50] = new double[,] { { 0, 0.340 }, { 11, 0.660 } };
        decaytypes[30, 49] = new double[,] { { 0, 0.983 }, { 11, 0.017 } };
        decaytypes[31, 48] = new double[,] { { 0, 1.000 } };
        decaytypes[32, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 46] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 45] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 44] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[36, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 39] = new double[,] { { 15, 1.000 } };
        decaytypes[41, 38] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[28, 52] = new double[,] { { 11, 0.600 }, { 12, 0.400 } };
        decaytypes[29, 51] = new double[,] { { 0, 0.400 }, { 11, 0.400 }, { 12, 0.200 } };
        decaytypes[30, 50] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[31, 49] = new double[,] { { 0, 0.991 }, { 11, 0.009 } };
        decaytypes[32, 48] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 46] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 45] = new double[,] { { 0, 0.917 }, { 1, 0.083 } };
        decaytypes[36, 44] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 40] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 39] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[29, 52] = new double[,] { { 11, 0.700 }, { 12, 0.300 } };
        decaytypes[30, 51] = new double[,] { { 0, 0.909 }, { 11, 0.091 } };
        decaytypes[31, 50] = new double[,] { { 0, 0.881 }, { 11, 0.119 } };
        decaytypes[32, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 48] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 46] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[36, 45] = new double[,] { { 18, 1.000 } };
        decaytypes[37, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 41] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 40] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[42, 39] = new double[,] { { 15, 1.000 } };
        decaytypes[29, 53] = new double[,] { { 0, 0.100 }, { 11, 0.300 }, { 12, 0.600 } };
        decaytypes[30, 52] = new double[,] { { 0, 0.310 }, { 11, 0.690 } };
        decaytypes[31, 51] = new double[,] { { 0, 0.787 }, { 11, 0.213 } };
        decaytypes[32, 50] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 48] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[35, 47] = new double[,] { { 0, 1.000 } };
        decaytypes[36, 46] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[38, 44] = new double[,] { { 18, 1.000 } };
        decaytypes[39, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 41] = new double[,] { { 15, 1.000 } };
        decaytypes[42, 40] = new double[,] { { 15, 1.000 } };
        decaytypes[30, 53] = new double[,] { { 0, 0.870 }, { 11, 0.100 }, { 12, 0.030 } };
        decaytypes[31, 52] = new double[,] { { 0, 0.372 }, { 11, 0.628 } };
        decaytypes[32, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[33, 50] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 48] = new double[,] { { 0, 1.000 } };
        decaytypes[36, 47] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 46] = new double[,] { { 18, 1.000 } };
        decaytypes[38, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[39, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 43] = new double[,] { { 15, 1.000 } };
        decaytypes[41, 42] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 41] = new double[,] { { 15, 1.000 } };
        decaytypes[43, 40] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[30, 54] = new double[,] { { 0, 0.560 }, { 11, 0.400 }, { 12, 0.040 } };
        decaytypes[31, 53] = new double[,] { { 0, 0.580 }, { 11, 0.400 }, { 12, 0.020 } };
        decaytypes[32, 52] = new double[,] { { 0, 0.893 }, { 11, 0.107 } };
        decaytypes[33, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[34, 50] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[36, 48] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 47] = new double[,] { { 0, 0.039 }, { 1, 0.961 } };
        decaytypes[38, 46] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[39, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 42] = new double[,] { { 15, 1.000 } };
        decaytypes[43, 41] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[30, 55] = new double[,] { { 0, 0.630 }, { 11, 0.300 }, { 12, 0.070 } };
        decaytypes[31, 54] = new double[,] { { 0, 0.590 }, { 11, 0.350 }, { 12, 0.060 } };
        decaytypes[32, 53] = new double[,] { { 0, 0.835 }, { 11, 0.165 } };
        decaytypes[33, 52] = new double[,] { { 0, 0.371 }, { 11, 0.629 } };
        decaytypes[34, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 50] = new double[,] { { 0, 1.000 } };
        decaytypes[36, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 48] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[38, 47] = new double[,] { { 18, 1.000 } };
        decaytypes[39, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 42] = new double[,] { { 3, 1.000 } };
        decaytypes[44, 41] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[31, 55] = new double[,] { { 0, 0.200 }, { 11, 0.600 }, { 12, 0.200 } };
        decaytypes[32, 54] = new double[,] { { 0, 0.550 }, { 11, 0.450 } };
        decaytypes[33, 53] = new double[,] { { 0, 0.645 }, { 11, 0.355 } };
        decaytypes[34, 52] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[36, 50] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[37, 49] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 48] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[39, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 43] = new double[,] { { 15, 1.000 } };
        decaytypes[44, 42] = new double[,] { { 15, 1.000 } };
        decaytypes[31, 56] = new double[,] { { 0, 0.030 }, { 11, 0.900 }, { 12, 0.070 } };
        decaytypes[32, 55] = new double[,] { { 0, 0.960 }, { 11, 0.030 }, { 12, 0.010 } };
        decaytypes[33, 54] = new double[,] { { 0, 0.846 }, { 11, 0.154 } };
        decaytypes[34, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[35, 52] = new double[,] { { 0, 0.974 }, { 11, 0.026 } };
        decaytypes[36, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 50] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 49] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[39, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 45] = new double[,] { { 1, 0.850 }, { 15, 0.150 } };
        decaytypes[43, 44] = new double[,] { { 15, 1.000 } };
        decaytypes[44, 43] = new double[,] { { 15, 1.000 } };
        decaytypes[32, 56] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[33, 55] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[34, 54] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[35, 53] = new double[,] { { 0, 0.934 }, { 11, 0.066 } };
        decaytypes[36, 52] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 50] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[39, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[40, 48] = new double[,] { { 18, 1.000 } };
        decaytypes[41, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 45] = new double[,] { { 15, 1.000 } };
        decaytypes[44, 44] = new double[,] { { 15, 1.000 } };
        decaytypes[45, 43] = new double[,] { { 1, 1.000 } };
        decaytypes[32, 57] = new double[,] { { 0, 0.780 }, { 11, 0.200 }, { 12, 0.020 } };
        decaytypes[33, 56] = new double[,] { { 11, 1.000 } };
        decaytypes[34, 55] = new double[,] { { 0, 0.922 }, { 11, 0.078 } };
        decaytypes[35, 54] = new double[,] { { 0, 0.862 }, { 11, 0.138 } };
        decaytypes[36, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 52] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 50] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[40, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[41, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 45] = new double[,] { { 1, 0.969 }, { 15, 0.031 } };
        decaytypes[45, 44] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[32, 58] = new double[,] { { 0, 0.480 }, { 11, 0.500 }, { 12, 0.020 } };
        decaytypes[33, 57] = new double[,] { { 0, 0.670 }, { 11, 0.300 }, { 12, 0.030 } };
        decaytypes[34, 56] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[35, 55] = new double[,] { { 0, 0.748 }, { 11, 0.252 } };
        decaytypes[36, 54] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 52] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 51] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 50] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[41, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 45] = new double[,] { { 15, 1.000 } };
        decaytypes[46, 44] = new double[,] { { 1, 1.000 } };
        decaytypes[33, 58] = new double[,] { { 0, 0.070 }, { 11, 0.900 }, { 12, 0.030 } };
        decaytypes[34, 57] = new double[,] { { 0, 0.790 }, { 11, 0.210 } };
        decaytypes[35, 56] = new double[,] { { 0, 0.805 }, { 11, 0.195 } };
        decaytypes[36, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 54] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 52] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 51] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[41, 50] = new double[,] { { 18, 1.000 } };
        decaytypes[42, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[43, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 47] = new double[,] { { 15, 1.000 } };
        decaytypes[45, 46] = new double[,] { { 1, 0.987 }, { 15, 0.013 } };
        decaytypes[46, 45] = new double[,] { { 15, 1.000 } };
        decaytypes[33, 59] = new double[,] { { 11, 0.600 }, { 12, 0.400 } };
        decaytypes[34, 58] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[35, 57] = new double[,] { { 0, 0.669 }, { 11, 0.331 } };
        decaytypes[36, 56] = new double[,] { { 0, 1.000 } };
        decaytypes[37, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[38, 54] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 52] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[41, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[42, 50] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 47] = new double[,] { { 1, 0.981 }, { 15, 0.019 } };
        decaytypes[46, 46] = new double[,] { { 15, 1.000 } };
        decaytypes[47, 45] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 59] = new double[,] { { 0, 0.680 }, { 11, 0.300 }, { 12, 0.020 } };
        decaytypes[35, 58] = new double[,] { { 0, 0.450 }, { 11, 0.550 } };
        decaytypes[36, 57] = new double[,] { { 0, 0.980 }, { 11, 0.02 } };
        decaytypes[37, 56] = new double[,] { { 0, 0.986 }, { 11, 0.014 } };
        decaytypes[38, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 54] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 52] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[42, 51] = new double[,] { { 18, 1.000 } };
        decaytypes[43, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 49] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 47] = new double[,] { { 1, 0.925 }, { 15, 0.075 } };
        decaytypes[47, 46] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[34, 60] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[35, 59] = new double[,] { { 0, 0.290 }, { 11, 0.680 }, { 12, 0.030 } };
        decaytypes[36, 58] = new double[,] { { 0, 0.989 }, { 11, 0.011 } };
        decaytypes[37, 57] = new double[,] { { 0, 0.895 }, { 11, 0.105 } };
        decaytypes[38, 56] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 54] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[41, 53] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 52] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 49] = new double[,] { { 1, 0.982 }, { 15, 0.018 } };
        decaytypes[46, 48] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 47] = new double[,] { { 15, 1.000 } };
        decaytypes[48, 46] = new double[,] { { 1, 1.000 } };
        decaytypes[34, 61] = new double[,] { { 0, 0.870 }, { 11, 0.100 }, { 12, 0.030 } };
        decaytypes[35, 60] = new double[,] { { 0, 0.300 }, { 11, 0.700 } };
        decaytypes[36, 59] = new double[,] { { 0, 0.971 }, { 11, 0.029 } };
        decaytypes[37, 58] = new double[,] { { 0, 0.913 }, { 11, 0.087 } };
        decaytypes[38, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 56] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 54] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 53] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 49] = new double[,] { { 15, 1.000 } };
        decaytypes[47, 48] = new double[,] { { 1, 0.975 }, { 15, 0.025 } };
        decaytypes[48, 47] = new double[,] { { 15, 1.000 } };
        decaytypes[35, 61] = new double[,] { { 0, 0.440 }, { 11, 0.500 }, { 12, 0.060 } };
        decaytypes[36, 60] = new double[,] { { 0, 0.963 }, { 11, 0.037 } };
        decaytypes[37, 59] = new double[,] { { 0, 0.867 }, { 11, 0.133 } };
        decaytypes[38, 58] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 56] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[41, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 54] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[44, 52] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 49] = new double[,] { { 1, 0.931 }, { 15, 0.069 } };
        decaytypes[48, 48] = new double[,] { { 1, 0.945 }, { 15, 0.055 } };
        decaytypes[49, 47] = new double[,] { { 1, 1.000 } };
        decaytypes[35, 62] = new double[,] { { 0, 0.050 }, { 11, 0.900 }, { 12, 0.050 } };
        decaytypes[36, 61] = new double[,] { { 0, 0.933 }, { 11, 0.067 } };
        decaytypes[37, 60] = new double[,] { { 0, 0.745 }, { 11, 0.255 } };
        decaytypes[38, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 58] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 56] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 55] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 54] = new double[,] { { 18, 1.000 } };
        decaytypes[44, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 49] = new double[,] { { 1, 0.882 }, { 15, 0.118 } };
        decaytypes[49, 48] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[35, 63] = new double[,] { { 0, 0.100 }, { 11, 0.700 }, { 12, 0.200 } };
        decaytypes[36, 62] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[37, 61] = new double[,] { { 0, 0.862 }, { 11, 0.138 } };
        decaytypes[38, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[40, 58] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 56] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 55] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 54] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 50] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 49] = new double[,] { { 1, 0.944 }, { 15, 0.056 } };
        decaytypes[36, 63] = new double[,] { { 0, 0.870 }, { 11, 0.110 }, { 12, 0.020 } };
        decaytypes[37, 62] = new double[,] { { 0, 0.842 }, { 11, 0.158 } };
        decaytypes[38, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[39, 60] = new double[,] { { 0, 0.983 }, { 11, 0.017 } };
        decaytypes[40, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 58] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 56] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 55] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 54] = new double[,] { { 1, 1.000 } };
        decaytypes[46, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 51] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 50] = new double[,] { { 1, 0.991 }, { 15, 0.009 } };
        decaytypes[50, 49] = new double[,] { { 15, 1.000 } };
        decaytypes[36, 64] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[37, 63] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[38, 62] = new double[,] { { 0, 0.992 }, { 11, 0.008 } };
        decaytypes[39, 61] = new double[,] { { 0, 0.991 }, { 11, 0.009 } };
        decaytypes[40, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 58] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[43, 57] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 56] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 55] = new double[,] { { 18, 1.000 } };
        decaytypes[46, 54] = new double[,] { { 18, 1.000 } };
        decaytypes[47, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 51] = new double[,] { { 1, 0.984 }, { 15, 0.016 } };
        decaytypes[50, 50] = new double[,] { { 1, 0.830 }, { 15, 0.170 } };
        decaytypes[36, 65] = new double[,] { { 0, 0.780 }, { 11, 0.200 }, { 12, 0.020 } };
        decaytypes[37, 64] = new double[,] { { 0, 0.720 }, { 11, 0.280 } };
        decaytypes[38, 63] = new double[,] { { 0, 0.976 }, { 11, 0.024 } };
        decaytypes[39, 62] = new double[,] { { 0, 0.981 }, { 11, 0.019 } };
        decaytypes[40, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 58] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 57] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 56] = new double[,] { { 18, 1.000 } };
        decaytypes[46, 55] = new double[,] { { 1, 1.000 } };
        decaytypes[47, 54] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 52] = new double[,] { { 15, 1.000 } };
        decaytypes[50, 51] = new double[,] { { 1, 0.790 }, { 15, 0.210 } };
        decaytypes[37, 65] = new double[,] { { 0, 0.330 }, { 11, 0.650 }, { 12, 0.020 } };
        decaytypes[38, 64] = new double[,] { { 0, 0.945 }, { 11, 0.055 } };
        decaytypes[39, 63] = new double[,] { { 0, 0.951 }, { 11, 0.049 } };
        decaytypes[40, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[41, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 58] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 57] = new double[,] { { 0, 0.220 }, { 1, 0.780 } };
        decaytypes[46, 56] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 55] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 54] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 53] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 52] = new double[,] { { 1, 1.000 } };
        decaytypes[37, 66] = new double[,] { { 0, 0.480 }, { 11, 0.500 }, { 12, 0.020 } };
        decaytypes[38, 65] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[39, 64] = new double[,] { { 0, 0.920 }, { 11, 0.080 } };
        decaytypes[40, 63] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[41, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 58] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[46, 57] = new double[,] { { 18, 1.000 } };
        decaytypes[47, 56] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 55] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 54] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 53] = new double[,] { { 1, 0.988 }, { 15, 0.012 } };
        decaytypes[51, 52] = new double[,] { { 3, 1.000 } };
        decaytypes[38, 66] = new double[,] { { 0, 0.910 }, { 11, 0.090 } };
        decaytypes[39, 65] = new double[,] { { 0, 0.660 }, { 11, 0.340 } };
        decaytypes[40, 64] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[41, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[42, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 60] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[45, 59] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 58] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 57] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 56] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 55] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 54] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 53] = new double[,] { { 1, 0.449 }, { 3, 0.034 }, { 8, 0.483 }, { 15, 0.034 } };
        decaytypes[38, 67] = new double[,] { { 0, 0.890 }, { 11, 0.100 }, { 12, 0.010 } };
        decaytypes[39, 66] = new double[,] { { 0, 0.180 }, { 11, 0.820 } };
        decaytypes[40, 65] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[41, 64] = new double[,] { { 0, 0.983 }, { 11, 0.017 } };
        decaytypes[42, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 60] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 59] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 58] = new double[,] { { 1, 1.000 } };
        decaytypes[48, 57] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 56] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 55] = new double[,] { { 15, 1.000 } };
        decaytypes[51, 54] = new double[,] { { 15, 1.000 } };
        decaytypes[52, 53] = new double[,] { { 8, 1.000 } };
        decaytypes[38, 68] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[39, 67] = new double[,] { { 0, 0.795 }, { 11, 0.200 }, { 12, 0.005 } };
        decaytypes[40, 66] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[41, 65] = new double[,] { { 0, 0.955 }, { 11, 0.045 } };
        decaytypes[42, 64] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 60] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 59] = new double[,] { { 0, 0.005 }, { 1, 0.995 } };
        decaytypes[48, 58] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 57] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 56] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 55] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 54] = new double[,] { { 8, 1.000 } };
        decaytypes[38, 69] = new double[,] { { 0, 0.670 }, { 11, 0.300 }, { 12, 0.030 } };
        decaytypes[39, 68] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[40, 67] = new double[,] { { 0, 0.770 }, { 11, 0.230 } };
        decaytypes[41, 66] = new double[,] { { 0, 0.926 }, { 11, 0.074 } };
        decaytypes[42, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 64] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 62] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 61] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 60] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[48, 59] = new double[,] { { 1, 1.000 } };
        decaytypes[49, 58] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 57] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 56] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 55] = new double[,] { { 8, 0.412 }, { 15, 0.588 } };
        decaytypes[53, 54] = new double[,] { { 8, 1.000 } };
        decaytypes[39, 69] = new double[,] { { 0, 0.680 }, { 11, 0.300 }, { 12, 0.020 } };
        decaytypes[40, 68] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[41, 67] = new double[,] { { 0, 0.937 }, { 11, 0.063 } };
        decaytypes[42, 66] = new double[,] { { 0, 0.995 }, { 11, 0.005 } };
        decaytypes[43, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 64] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 62] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 61] = new double[,] { { 0, 0.972 }, { 1, 0.028 } };
        decaytypes[48, 60] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 59] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 58] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 57] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 56] = new double[,] { { 1, 0.486 }, { 8, 0.490 }, { 15, 0.024 } };
        decaytypes[53, 55] = new double[,] { { 1, 0.085 }, { 3, 0.005 }, { 8, 0.909 }, { 15, 0.001 } };
        decaytypes[39, 70] = new double[,] { { 0, 0.385 }, { 11, 0.600 }, { 12, 0.015 } };
        decaytypes[40, 69] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[41, 68] = new double[,] { { 0, 0.690 }, { 11, 0.310 } };
        decaytypes[42, 67] = new double[,] { { 0, 0.987 }, { 11, 0.013 } };
        decaytypes[43, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 64] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 62] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[48, 61] = new double[,] { { 18, 1.000 } };
        decaytypes[49, 60] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 59] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 58] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 57] = new double[,] { { 1, 0.867 }, { 8, 0.039 }, { 15, 0.094 } };
        decaytypes[53, 56] = new double[,] { { 3, 1.000 } };
        decaytypes[54, 55] = new double[,] { { 8, 0.500 }, { 15, 0.500 } };
        decaytypes[40, 70] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[41, 69] = new double[,] { { 0, 0.600 }, { 11, 0.400 } };
        decaytypes[42, 68] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[43, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[44, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 64] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[47, 63] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 62] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[50, 60] = new double[,] { { 18, 1.000 } };
        decaytypes[51, 59] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 58] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 57] = new double[,] { { 1, 0.709 }, { 8, 0.170 }, { 10, 0.011 }, { 15, 0.110 } };
        decaytypes[54, 56] = new double[,] { { 8, 0.390 }, { 15, 0.610 } };
        decaytypes[40, 71] = new double[,] { { 0, 0.890 }, { 11, 0.100 }, { 12, 0.010 } };
        decaytypes[41, 70] = new double[,] { { 0, 0.100 }, { 11, 0.900 } };
        decaytypes[42, 69] = new double[,] { { 0, 0.880 }, { 11, 0.120 } };
        decaytypes[43, 68] = new double[,] { { 0, 0.992 }, { 11, 0.008 } };
        decaytypes[44, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 64] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 63] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 62] = new double[,] { { 18, 1.000 } };
        decaytypes[50, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 60] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 59] = new double[,] { { 15, 1.000 } };
        decaytypes[53, 58] = new double[,] { { 15, 1.000 } };
        decaytypes[54, 57] = new double[,] { { 8, 0.094 }, { 15, 0.906 } };
        decaytypes[55, 56] = new double[,] { { 3, 1.000 } };
        decaytypes[40, 72] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[41, 71] = new double[,] { { 0, 0.290 }, { 11, 0.700 }, { 12, 0.010 } };
        decaytypes[42, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[43, 69] = new double[,] { { 0, 0.985 }, { 11, 0.015 } };
        decaytypes[44, 68] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 64] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 63] = new double[,] { { 0, 0.426 }, { 1, 0.574 } };
        decaytypes[50, 62] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 60] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 59] = new double[,] { { 1, 0.991 }, { 15, 0.009 } };
        decaytypes[54, 58] = new double[,] { { 8, 0.012 }, { 15, 0.988 } };
        decaytypes[55, 57] = new double[,] { { 3, 1.000 } };
        decaytypes[41, 72] = new double[,] { { 0, 0.080 }, { 11, 0.900 }, { 12, 0.020 } };
        decaytypes[42, 71] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[43, 70] = new double[,] { { 0, 0.979 }, { 11, 0.021 } };
        decaytypes[44, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 68] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 65] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 64] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[50, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[51, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 60] = new double[,] { { 10, 1.000 } };
        decaytypes[54, 59] = new double[,] { { 1, 0.930 }, { 15, 0.070 } };
        decaytypes[55, 58] = new double[,] { { 3, 1.000 } };
        decaytypes[56, 57] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[41, 73] = new double[,] { { 0, 0.440 }, { 11, 0.500 }, { 12, 0.060 } };
        decaytypes[42, 72] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[43, 71] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[44, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 68] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 66] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 65] = new double[,] { { 0, 0.995 }, { 1, 0.005 } };
        decaytypes[50, 64] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 61] = new double[,] { { 15, 1.000 } };
        decaytypes[54, 60] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 59] = new double[,] { { 1, 0.913 }, { 15, 0.087 } };
        decaytypes[56, 58] = new double[,] { { 1, 0.793 }, { 8, 0.009 }, { 15, 0.198 } };
        decaytypes[41, 74] = new double[,] { { 0, 0.390 }, { 11, 0.600 }, { 12, 0.010 } };
        decaytypes[42, 73] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[43, 72] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[44, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[46, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 68] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 66] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 65] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 60] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 59] = new double[,] { { 1, 0.850 }, { 15, 0.150 } };
        decaytypes[42, 74] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[43, 73] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[44, 72] = new double[,] { { 0, 1.000 } };
        decaytypes[45, 71] = new double[,] { { 0, 0.979 }, { 11, 0.021 } };
        decaytypes[46, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 68] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[49, 67] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 66] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 65] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 61] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 60] = new double[,] { { 1, 0.970 }, { 15, 0.030 } };
        decaytypes[57, 59] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[42, 75] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[43, 74] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[44, 73] = new double[,] { { 0, 0.995 }, { 11, 0.005 } };
        decaytypes[45, 72] = new double[,] { { 0, 0.924 }, { 11, 0.076 } };
        decaytypes[46, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 68] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 67] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 65] = new double[,] { { 18, 1.000 } };
        decaytypes[53, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 61] = new double[,] { { 1, 0.870 }, { 15, 0.130 } };
        decaytypes[57, 60] = new double[,] { { 3, 0.95 }, { 15, 0.05 } };
        decaytypes[42, 76] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[43, 75] = new double[,] { { 0, 0.694 }, { 11, 0.300 }, { 12, 0.006 } };
        decaytypes[44, 74] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[45, 73] = new double[,] { { 0, 0.969 }, { 11, 0.031 } };
        decaytypes[46, 72] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 69] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 68] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 66] = new double[,] { { 18, 1.000 } };
        decaytypes[53, 65] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 63] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 61] = new double[,] { { 15, 1.000 } };
        decaytypes[43, 76] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[44, 75] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[45, 74] = new double[,] { { 0, 0.936 }, { 11, 0.064 } };
        decaytypes[46, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 72] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 70] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 69] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 68] = new double[,] { { 18, 1.000 } };
        decaytypes[52, 67] = new double[,] { { 18, 1.000 } };
        decaytypes[53, 66] = new double[,] { { 18, 1.000 } };
        decaytypes[54, 65] = new double[,] { { 18, 1.000 } };
        decaytypes[55, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 63] = new double[,] { { 1, 0.750 }, { 15, 0.250 } };
        decaytypes[57, 62] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 61] = new double[,] { { 15, 1.000 } };
        decaytypes[43, 77] = new double[,] { { 0, 0.680 }, { 11, 0.300 }, { 12, 0.020 } };
        decaytypes[44, 76] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[45, 75] = new double[,] { { 0, 0.946 }, { 11, 0.054 } };
        decaytypes[46, 74] = new double[,] { { 0, 0.993 }, { 11, 0.007 } };
        decaytypes[47, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 72] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 70] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[52, 68] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 65] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 64] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 63] = new double[,] { { 15, 1.000 } };
        decaytypes[58, 62] = new double[,] { { 15, 1.000 } };
        decaytypes[43, 78] = new double[,] { { 0, 0.493 }, { 11, 0.500 }, { 12, 0.007 } };
        decaytypes[44, 77] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[45, 76] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[46, 75] = new double[,] { { 0, 0.992 }, { 11, 0.008 } };
        decaytypes[47, 74] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 72] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 71] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 70] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[52, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[53, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 65] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 64] = new double[,] { { 15, 1.000 } };
        decaytypes[58, 63] = new double[,] { { 1, 0.990 }, { 15, 0.010 } };
        decaytypes[59, 62] = new double[,] { { 3, 1.000 } };
        decaytypes[44, 78] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[45, 77] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[46, 76] = new double[,] { { 0, 0.975 }, { 11, 0.025 } };
        decaytypes[47, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[48, 74] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 72] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 71] = new double[,] { { 0, 0.976 }, { 1, 0.024 } };
        decaytypes[52, 70] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 69] = new double[,] { { 18, 1.000 } };
        decaytypes[54, 68] = new double[,] { { 18, 1.000 } };
        decaytypes[55, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 65] = new double[,] { { 15, 1.000 } };
        decaytypes[58, 64] = new double[,] { { 15, 1.000 } };
        decaytypes[59, 63] = new double[,] { { 15, 1.000 } };
        decaytypes[44, 79] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[45, 78] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[46, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 76] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[48, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 74] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 72] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[52, 71] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 65] = new double[,] { { 15, 1.000 } };
        decaytypes[59, 64] = new double[,] { { 15, 1.000 } };
        decaytypes[44, 80] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[45, 79] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[46, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 77] = new double[,] { { 0, 0.987 }, { 11, 0.013 } };
        decaytypes[48, 76] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 74] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[51, 73] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 72] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[54, 70] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 67] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 66] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 65] = new double[,] { { 15, 1.000 } };
        decaytypes[60, 64] = new double[,] { { 15, 1.000 } };
        decaytypes[45, 80] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[46, 79] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[47, 78] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[48, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 76] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 74] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 73] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 72] = new double[,] { { 18, 1.000 } };
        decaytypes[54, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[55, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 67] = new double[,] { { 15, 1.000 } };
        decaytypes[59, 66] = new double[,] { { 15, 1.000 } };
        decaytypes[60, 65] = new double[,] { { 1, 1.000 } };
        decaytypes[45, 81] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[46, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[47, 79] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[48, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 76] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 74] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 73] = new double[,] { { 0, 0.473 }, { 1, 0.527 } };
        decaytypes[54, 72] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 67] = new double[,] { { 15, 1.000 } };
        decaytypes[60, 66] = new double[,] { { 15, 1.000 } };
        decaytypes[61, 65] = new double[,] { { 15, 1.000 } };
        decaytypes[45, 82] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[46, 81] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[47, 80] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[48, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 76] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 75] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 74] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[54, 73] = new double[,] { { 18, 1.000 } };
        decaytypes[55, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 69] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 67] = new double[,] { { 15, 1.000 } };
        decaytypes[61, 66] = new double[,] { { 1, 0.500 }, { 3, 0.500 } };
        decaytypes[46, 82] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[47, 81] = new double[,] { { 0, 0.920 }, { 11, 0.080 } };
        decaytypes[48, 80] = new double[,] { { 0, 0.993 }, { 11, 0.007 } };
        decaytypes[49, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 76] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 75] = new double[,] { { 0, 0.931 }, { 1, 0.069 } };
        decaytypes[54, 74] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 72] = new double[,] { { 18, 1.000 } };
        decaytypes[57, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[60, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 67] = new double[,] { { 15, 1.000 } };
        decaytypes[62, 66] = new double[,] { { 15, 1.000 } };
        decaytypes[46, 83] = new double[,] { { 0, 0.080 }, { 11, 0.900 }, { 12, 0.020 } };
        decaytypes[47, 82] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[48, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[49, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[50, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 76] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 75] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[61, 68] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[62, 67] = new double[,] { { 15, 1.000 } };
        decaytypes[47, 83] = new double[,] { { 0, 0.080 }, { 11, 0.900 }, { 12, 0.020 } };
        decaytypes[48, 82] = new double[,] { { 0, 0.965 }, { 11, 0.035 } };
        decaytypes[49, 81] = new double[,] { { 0, 0.991 }, { 11, 0.009 } };
        decaytypes[50, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 78] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[53, 77] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 76] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 75] = new double[,] { { 0, 0.016 }, { 1, 0.984 } };
        decaytypes[56, 74] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[62, 68] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 67] = new double[,] { { 3, 0.990 }, { 15, 0.01 } };
        decaytypes[47, 84] = new double[,] { { 11, 0.900 }, { 12, 0.100 } };
        decaytypes[48, 83] = new double[,] { { 0, 0.965 }, { 11, 0.035 } };
        decaytypes[49, 82] = new double[,] { { 0, 0.978 }, { 11, 0.022 } };
        decaytypes[50, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 78] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 77] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 76] = new double[,] { { 18, 1.000 } };
        decaytypes[56, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 71] = new double[,] { { 15, 1.000 } };
        decaytypes[61, 70] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[63, 68] = new double[,] { { 3, 0.471 }, { 15, 0.529 } };
        decaytypes[47, 85] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[48, 84] = new double[,] { { 0, 0.400 }, { 11, 0.600 } };
        decaytypes[49, 83] = new double[,] { { 0, 0.937 }, { 11, 0.063 } };
        decaytypes[50, 82] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 78] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 77] = new double[,] { { 0, 0.019 }, { 1, 0.981 } };
        decaytypes[56, 76] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 71] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 70] = new double[,] { { 15, 1.000 } };
        decaytypes[63, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[48, 85] = new double[,] { { 0, 0.095 }, { 11, 0.005 }, { 12, 0.900 } };
        decaytypes[49, 84] = new double[,] { { 0, 0.150 }, { 11, 0.850 } };
        decaytypes[50, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[51, 82] = new double[,] { { 0, 1.000 } };
        decaytypes[52, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 78] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[56, 77] = new double[,] { { 18, 1.000 } };
        decaytypes[57, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 71] = new double[,] { { 15, 1.000 } };
        decaytypes[63, 70] = new double[,] { { 15, 1.000 } };
        decaytypes[64, 69] = new double[,] { { 15, 1.000 } };
        decaytypes[48, 86] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[49, 85] = new double[,] { { 0, 0.310 }, { 11, 0.650 }, { 12, 0.040 } };
        decaytypes[50, 84] = new double[,] { { 0, 0.830 }, { 11, 0.170 } };
        decaytypes[51, 83] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[52, 82] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 80] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 79] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 78] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 76] = new double[,] { { 18, 1.000 } };
        decaytypes[59, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 72] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 71] = new double[,] { { 15, 1.000 } };
        decaytypes[64, 70] = new double[,] { { 15, 1.000 } };
        decaytypes[49, 86] = new double[,] { { 0, 0.020 }, { 11, 0.900 }, { 12, 0.080 } };
        decaytypes[50, 85] = new double[,] { { 0, 0.730 }, { 11, 0.210 }, { 12, 0.060 } };
        decaytypes[51, 84] = new double[,] { { 0, 0.780 }, { 11, 0.220 } };
        decaytypes[52, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[53, 82] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 80] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 79] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 72] = new double[,] { { 15, 1.000 } };
        decaytypes[64, 71] = new double[,] { { 1, 0.820 }, { 15, 0.180 } };
        decaytypes[65, 70] = new double[,] { { 1, 0.500 }, { 3, 0.500 } };
        decaytypes[49, 87] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[50, 86] = new double[,] { { 0, 0.700 }, { 11, 0.280 }, { 12, 0.020 } };
        decaytypes[51, 85] = new double[,] { { 0, 0.737 }, { 11, 0.163 }, { 12, 0.100 } };
        decaytypes[52, 84] = new double[,] { { 0, 0.987 }, { 11, 0.013 } };
        decaytypes[53, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[54, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[55, 81] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 80] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 78] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[59, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 73] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 72] = new double[,] { { 15, 1.000 } };
        decaytypes[65, 71] = new double[,] { { 15, 1.000 } };
        decaytypes[49, 88] = new double[,] { { 0, 0.100 }, { 12, 0.900 } };
        decaytypes[50, 87] = new double[,] { { 0, 0.100 }, { 11, 0.500 }, { 12, 0.400 } };
        decaytypes[51, 86] = new double[,] { { 0, 0.510 }, { 11, 0.490 } };
        decaytypes[52, 85] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[53, 84] = new double[,] { { 0, 0.922 }, { 11, 0.078 } };
        decaytypes[54, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 82] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 81] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 80] = new double[,] { { 18, 1.000 } };
        decaytypes[58, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[59, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 73] = new double[,] { { 15, 1.000 } };
        decaytypes[65, 72] = new double[,] { { 1, 0.500 }, { 3, 0.500 } };
        decaytypes[50, 88] = new double[,] { { 0, 0.590 }, { 11, 0.360 }, { 12, 0.050 } };
        decaytypes[51, 87] = new double[,] { { 0, 0.260 }, { 11, 0.720 }, { 12, 0.020 } };
        decaytypes[52, 86] = new double[,] { { 0, 0.937 }, { 11, 0.063 } };
        decaytypes[53, 85] = new double[,] { { 0, 0.945 }, { 11, 0.055 } };
        decaytypes[54, 84] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[57, 81] = new double[,] { { 0, 0.344 }, { 1, 0.656 } };
        decaytypes[58, 80] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[59, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 74] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 73] = new double[,] { { 15, 1.000 } };
        decaytypes[66, 72] = new double[,] { { 15, 1.000 } };
        decaytypes[50, 89] = new double[,] { { 11, 0.800 }, { 12, 0.200 } };
        decaytypes[51, 88] = new double[,] { { 0, 0.070 }, { 11, 0.900 }, { 12, 0.030 } };
        decaytypes[52, 87] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[53, 86] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[54, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 84] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[58, 81] = new double[,] { { 18, 1.000 } };
        decaytypes[59, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[60, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 75] = new double[,] { { 15, 1.000 } };
        decaytypes[65, 74] = new double[,] { { 15, 1.000 } };
        decaytypes[66, 73] = new double[,] { { 15, 1.000 } };
        decaytypes[51, 89] = new double[,] { { 0, 0.400 }, { 11, 0.400 }, { 12, 0.200 } };
        decaytypes[52, 88] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[53, 87] = new double[,] { { 0, 0.907 }, { 11, 0.093 } };
        decaytypes[54, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 84] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[59, 81] = new double[,] { { 18, 1.000 } };
        decaytypes[60, 80] = new double[,] { { 18, 1.000 } };
        decaytypes[61, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 75] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 74] = new double[,] { { 15, 1.000 } };
        decaytypes[67, 73] = new double[,] { { 3, 0.995 }, { 15, 0.005 } };
        decaytypes[51, 90] = new double[,] { { 0, 0.070 }, { 11, 0.900 }, { 12, 0.030 } };
        decaytypes[52, 89] = new double[,] { { 0, 0.920 }, { 11, 0.080 } };
        decaytypes[53, 88] = new double[,] { { 0, 0.790 }, { 11, 0.210 } };
        decaytypes[54, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 84] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[60, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[61, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[62, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 75] = new double[,] { { 15, 1.000 } };
        decaytypes[67, 74] = new double[,] { { 3, 0.995 }, { 15, 0.005 } };
        decaytypes[52, 90] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[53, 89] = new double[,] { { 0, 0.790 }, { 11, 0.200 }, { 12, 0.010 } };
        decaytypes[54, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[55, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[56, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 84] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[59, 83] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 81] = new double[,] { { 18, 1.000 } };
        decaytypes[62, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 78] = new double[,] { { 18, 1.000 } };
        decaytypes[65, 77] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 75] = new double[,] { { 15, 1.000 } };
        decaytypes[68, 74] = new double[,] { { 3, 1.000 } };
        decaytypes[52, 91] = new double[,] { { 0, 0.780 }, { 11, 0.200 }, { 12, 0.020 } };
        decaytypes[53, 90] = new double[,] { { 0, 0.300 }, { 11, 0.700 } };
        decaytypes[54, 89] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[55, 88] = new double[,] { { 0, 0.984 }, { 11, 0.016 } };
        decaytypes[56, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 84] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 83] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 82] = new double[,] { { 18, 1.000 } };
        decaytypes[62, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[63, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 77] = new double[,] { { 15, 1.000 } };
        decaytypes[67, 76] = new double[,] { { 15, 1.000 } };
        decaytypes[68, 75] = new double[,] { { 15, 1.000 } };
        decaytypes[53, 91] = new double[,] { { 0, 0.590 }, { 11, 0.400 }, { 12, 0.010 } };
        decaytypes[54, 90] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[55, 89] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[56, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 85] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 84] = new double[,] { { 8, 1.000 } };
        decaytypes[61, 83] = new double[,] { { 18, 1.000 } };
        decaytypes[62, 82] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[63, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 79] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 78] = new double[,] { { 15, 1.000 } };
        decaytypes[67, 77] = new double[,] { { 15, 1.000 } };
        decaytypes[68, 76] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 75] = new double[,] { { 3, 1.000 } };
        decaytypes[53, 92] = new double[,] { { 0, 0.600 }, { 11, 0.400 } };
        decaytypes[54, 91] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[55, 90] = new double[,] { { 0, 0.853 }, { 11, 0.147 } };
        decaytypes[56, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 85] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 84] = new double[,] { { 18, 1.000 } };
        decaytypes[62, 83] = new double[,] { { 18, 1.000 } };
        decaytypes[63, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 79] = new double[,] { { 15, 1.000 } };
        decaytypes[67, 78] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 77] = new double[,] { { 15, 1.000 } };
        decaytypes[69, 76] = new double[,] { { 3, 1.000 } };
        decaytypes[54, 92] = new double[,] { { 0, 0.931 }, { 11, 0.069 } };
        decaytypes[55, 91] = new double[,] { { 0, 0.858 }, { 11, 0.142 } };
        decaytypes[56, 90] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 86] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 85] = new double[,] { { 0, 0.340 }, { 18, 0.660 } };
        decaytypes[62, 84] = new double[,] { { 8, 1.000 } };
        decaytypes[63, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 82] = new double[,] { { 18, 1.000 } };
        decaytypes[65, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 79] = new double[,] { { 15, 1.000 } };
        decaytypes[68, 78] = new double[,] { { 15, 1.000 } };
        decaytypes[69, 77] = new double[,] { { 3, 0.500 }, { 15, 0.500 } };
        decaytypes[54, 93] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[55, 92] = new double[,] { { 0, 0.715 }, { 11, 0.285 } };
        decaytypes[56, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 90] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 86] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 85] = new double[,] { { 8, 1.000 } };
        decaytypes[63, 84] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 79] = new double[,] { { 15, 1.000 } };
        decaytypes[69, 78] = new double[,] { { 1, 0.850 }, { 3, 0.150 } };
        decaytypes[54, 94] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[55, 93] = new double[,] { { 0, 0.749 }, { 11, 0.251 } };
        decaytypes[56, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[58, 90] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 88] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 87] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 86] = new double[,] { { 8, 1.000 } };
        decaytypes[63, 85] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 84] = new double[,] { { 8, 1.000 } };
        decaytypes[65, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 79] = new double[,] { { 15, 1.000 } };
        decaytypes[70, 78] = new double[,] { { 15, 1.000 } };
        decaytypes[55, 94] = new double[,] { { 0, 0.400 }, { 11, 0.600 } };
        decaytypes[56, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 92] = new double[,] { { 0, 0.986 }, { 11, 0.014 } };
        decaytypes[58, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 90] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 88] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 87] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[63, 86] = new double[,] { { 18, 1.000 } };
        decaytypes[64, 85] = new double[,] { { 1, 1.000 } };
        decaytypes[65, 84] = new double[,] { { 1, 0.833 }, { 8, 0.167 } };
        decaytypes[66, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 81] = new double[,] { { 1, 0.930 }, { 15, 0.070 } };
        decaytypes[69, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 79] = new double[,] { { 15, 1.000 } };
        decaytypes[55, 95] = new double[,] { { 0, 0.180 }, { 11, 0.800 }, { 12, 0.020 } };
        decaytypes[56, 94] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[57, 93] = new double[,] { { 0, 0.973 }, { 11, 0.027 } };
        decaytypes[58, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 90] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[61, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 88] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[63, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[64, 86] = new double[,] { { 8, 1.000 } };
        decaytypes[65, 85] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 84] = new double[,] { { 1, 0.640 }, { 8, 0.360 } };
        decaytypes[67, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 80] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 79] = new double[,] { { 1, 0.225 }, { 3, 0.775 } };
        decaytypes[55, 96] = new double[,] { { 0, 0.100 }, { 11, 0.900 } };
        decaytypes[56, 95] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[57, 94] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[58, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 90] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 89] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 88] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[64, 87] = new double[,] { { 18, 1.000 } };
        decaytypes[65, 86] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 85] = new double[,] { { 1, 0.947 }, { 8, 0.053 } };
        decaytypes[67, 84] = new double[,] { { 1, 0.820 }, { 8, 0.180 } };
        decaytypes[68, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 81] = new double[,] { { 15, 1.000 } };
        decaytypes[71, 80] = new double[,] { { 1, 0.270 }, { 3, 0.730 } };
        decaytypes[55, 97] = new double[,] { { 11, 1.000 } };
        decaytypes[56, 96] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[57, 95] = new double[,] { { 0, 0.500 }, { 11, 0.500 } };
        decaytypes[58, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 90] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[63, 89] = new double[,] { { 0, 0.279 }, { 1, 0.721 } };
        decaytypes[64, 88] = new double[,] { { 8, 1.000 } };
        decaytypes[65, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 86] = new double[,] { { 18, 1.000 } };
        decaytypes[67, 85] = new double[,] { { 1, 0.880 }, { 8, 0.120 } };
        decaytypes[68, 84] = new double[,] { { 1, 0.100 }, { 8, 0.900 } };
        decaytypes[69, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 81] = new double[,] { { 1, 0.850 }, { 15, 0.150 } };
        decaytypes[56, 97] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[57, 96] = new double[,] { { 0, 0.500 }, { 11, 0.500 } };
        decaytypes[58, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 90] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[64, 89] = new double[,] { { 18, 1.000 } };
        decaytypes[65, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 86] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 85] = new double[,] { { 1, 0.470 }, { 8, 0.530 } };
        decaytypes[69, 84] = new double[,] { { 1, 0.090 }, { 8, 0.910 } };
        decaytypes[70, 83] = new double[,] { { 1, 0.667 }, { 8, 0.333 } };
        decaytypes[71, 82] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[72, 81] = new double[,] { { 1, 1.000 } };
        decaytypes[56, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[57, 97] = new double[,] { { 0, 0.800 }, { 11, 0.200 } };
        decaytypes[58, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 92] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[63, 91] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 90] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[66, 88] = new double[,] { { 8, 1.000 } };
        decaytypes[67, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 86] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 85] = new double[,] { { 1, 0.460 }, { 8, 0.540 } };
        decaytypes[70, 84] = new double[,] { { 1, 0.074 }, { 8, 0.926 } };
        decaytypes[71, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 82] = new double[,] { { 1, 1.000 } };
        decaytypes[57, 98] = new double[,] { { 0, 0.400 }, { 11, 0.600 } };
        decaytypes[58, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[59, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 92] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 91] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 90] = new double[,] { { 18, 1.000 } };
        decaytypes[66, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 86] = new double[,] { { 1, 0.991 }, { 8, 0.009 } };
        decaytypes[70, 85] = new double[,] { { 1, 0.110 }, { 8, 0.890 } };
        decaytypes[71, 84] = new double[,] { { 1, 0.526 }, { 8, 0.474 } };
        decaytypes[72, 83] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[73, 82] = new double[,] { { 3, 1.000 } };
        decaytypes[57, 99] = new double[,] { { 11, 1.000 } };
        decaytypes[58, 98] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[59, 97] = new double[,] { { 0, 0.993 }, { 11, 0.007 } };
        decaytypes[60, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 93] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 92] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 91] = new double[,] { { 0, 0.500 }, { 1, 0.500 } };
        decaytypes[66, 90] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 87] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 86] = new double[,] { { 1, 0.900 }, { 8, 0.100 } };
        decaytypes[71, 85] = new double[,] { { 1, 0.048 }, { 8, 0.952 } };
        decaytypes[72, 84] = new double[,] { { 1, 0.508 }, { 8, 0.492 } };
        decaytypes[73, 83] = new double[,] { { 1, 0.290 }, { 3, 0.710 } };
        decaytypes[58, 99] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[59, 98] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[60, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 94] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 93] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 92] = new double[,] { { 18, 1.000 } };
        decaytypes[66, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[67, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 87] = new double[,] { { 1, 0.995 }, { 8, 0.005 } };
        decaytypes[71, 86] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[72, 85] = new double[,] { { 1, 0.130 }, { 8, 0.870 } };
        decaytypes[73, 84] = new double[,] { { 1, 0.010 }, { 3, 0.032 }, { 8, 0.958 } };
        decaytypes[74, 83] = new double[,] { { 1, 1.000 } };
        decaytypes[58, 100] = new double[,] { { 11, 1.000 } };
        decaytypes[59, 99] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[60, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 94] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 93] = new double[,] { { 0, 0.166 }, { 1, 0.834 } };
        decaytypes[66, 92] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 91] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[68, 90] = new double[,] { { 18, 1.000 } };
        decaytypes[69, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 87] = new double[,] { { 1, 0.991 }, { 8, 0.009 } };
        decaytypes[72, 86] = new double[,] { { 1, 0.557 }, { 8, 0.443 } };
        decaytypes[73, 85] = new double[,] { { 1, 0.510 }, { 8, 0.490 } };
        decaytypes[74, 84] = new double[,] { { 8, 1.000 } };
        decaytypes[59, 100] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[60, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 94] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[66, 93] = new double[,] { { 18, 1.000 } };
        decaytypes[67, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 88] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 87] = new double[,] { { 1, 0.650 }, { 8, 0.350 } };
        decaytypes[73, 86] = new double[,] { { 1, 0.746 }, { 8, 0.254 } };
        decaytypes[74, 85] = new double[,] { { 1, 0.549 }, { 8, 0.451 } };
        decaytypes[75, 84] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[59, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[60, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 96] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[65, 95] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 94] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 92] = new double[,] { { 18, 1.000 } };
        decaytypes[69, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 88] = new double[,] { { 1, 0.993 }, { 8, 0.007 } };
        decaytypes[73, 87] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[74, 86] = new double[,] { { 1, 0.535 }, { 8, 0.465 } };
        decaytypes[75, 85] = new double[,] { { 3, 0.890 }, { 8, 0.110 } };
        decaytypes[60, 101] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[61, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[62, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 96] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 95] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 94] = new double[,] { { 18, 1.000 } };
        decaytypes[68, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 89] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 88] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[74, 87] = new double[,] { { 1, 0.270 }, { 8, 0.730 } };
        decaytypes[75, 86] = new double[,] { { 3, 0.986 }, { 8, 0.014 } };
        decaytypes[76, 85] = new double[,] { { 8, 1.000 } };
        decaytypes[60, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[61, 101] = new double[,] { { 0, 0.992 }, { 11, 0.008 } };
        decaytypes[62, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 97] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 96] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[68, 94] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 89] = new double[,] { { 15, 1.000 } };
        decaytypes[74, 88] = new double[,] { { 1, 0.689 }, { 8, 0.311 } };
        decaytypes[75, 87] = new double[,] { { 1, 0.515 }, { 8, 0.485 } };
        decaytypes[76, 86] = new double[,] { { 8, 1.000 } };
        decaytypes[61, 102] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[62, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 98] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 97] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 96] = new double[,] { { 18, 1.000 } };
        decaytypes[68, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[69, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 90] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 89] = new double[,] { { 1, 0.877 }, { 8, 0.123 } };
        decaytypes[75, 88] = new double[,] { { 1, 0.758 }, { 8, 0.242 } };
        decaytypes[76, 87] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[61, 103] = new double[,] { { 11, 1.000 } };
        decaytypes[62, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 98] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[67, 97] = new double[,] { { 0, 0.400 }, { 18, 0.600 } };
        decaytypes[68, 96] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 95] = new double[,] { { 18, 1.000 } };
        decaytypes[70, 94] = new double[,] { { 18, 1.000 } };
        decaytypes[71, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 90] = new double[,] { { 1, 0.962 }, { 8, 0.038 } };
        decaytypes[75, 89] = new double[,] { { 1, 0.296 }, { 8, 0.704 } };
        decaytypes[76, 88] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[77, 87] = new double[,] { { 1, 0.334 }, { 3, 0.333 }, { 8, 0.333 } };
        decaytypes[62, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[64, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 98] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[68, 97] = new double[,] { { 18, 1.000 } };
        decaytypes[69, 96] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 91] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 90] = new double[,] { { 1, 0.877 }, { 8, 0.123 } };
        decaytypes[76, 89] = new double[,] { { 1, 0.526 }, { 8, 0.474 } };
        decaytypes[77, 88] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[62, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[63, 103] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[64, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 99] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 98] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 96] = new double[,] { { 18, 1.000 } };
        decaytypes[71, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 92] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 91] = new double[,] { { 1, 0.952 }, { 8, 0.048 } };
        decaytypes[76, 90] = new double[,] { { 1, 0.280 }, { 8, 0.720 } };
        decaytypes[77, 89] = new double[,] { { 3, 0.070 }, { 8, 0.930 } };
        decaytypes[78, 88] = new double[,] { { 8, 1.000 } };
        decaytypes[63, 104] = new double[,] { { 0, 0.970 }, { 11, 0.030 } };
        decaytypes[64, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 100] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 99] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 98] = new double[,] { { 18, 1.000 } };
        decaytypes[70, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[71, 96] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 92] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[76, 91] = new double[,] { { 1, 0.662 }, { 8, 0.338 } };
        decaytypes[77, 90] = new double[,] { { 1, 0.548 }, { 3, 0.216 }, { 8, 0.236 } };
        decaytypes[78, 89] = new double[,] { { 8, 1.000 } };
        decaytypes[63, 105] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[64, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 100] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[70, 98] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 96] = new double[,] { { 18, 1.000 } };
        decaytypes[73, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 93] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 92] = new double[,] { { 1, 0.570 }, { 8, 0.430 } };
        decaytypes[77, 91] = new double[,] { { 8, 0.500 }, { 15, 0.500 } };
        decaytypes[78, 90] = new double[,] { { 8, 1.000 } };
        decaytypes[64, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 100] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[70, 99] = new double[,] { { 18, 1.000 } };
        decaytypes[71, 98] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 96] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 94] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 93] = new double[,] { { 1, 0.863 }, { 8, 0.137 } };
        decaytypes[77, 92] = new double[,] { { 1, 0.654 }, { 8, 0.346 } };
        decaytypes[78, 91] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[79, 90] = new double[,] { { 1, 0.334 }, { 3, 0.333 }, { 8, 0.333 } };
        decaytypes[64, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[65, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[66, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 102] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[69, 101] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 100] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 98] = new double[,] { { 18, 1.000 } };
        decaytypes[73, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 96] = new double[,] { { 1, 0.990 }, { 8, 0.010 } };
        decaytypes[75, 95] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 94] = new double[,] { { 1, 0.913 }, { 8, 0.087 } };
        decaytypes[77, 93] = new double[,] { { 1, 0.951 }, { 8, 0.049 } };
        decaytypes[78, 92] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[79, 91] = new double[,] { { 3, 0.890 }, { 8, 0.110 } };
        decaytypes[65, 106] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[66, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 102] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 101] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 98] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 96] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 95] = new double[,] { { 1, 0.982 }, { 8, 0.018 } };
        decaytypes[77, 94] = new double[,] { { 1, 0.870 }, { 8, 0.130 } };
        decaytypes[78, 93] = new double[,] { { 1, 0.526 }, { 8, 0.474 } };
        decaytypes[79, 92] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[80, 91] = new double[,] { { 8, 1.000 } };
        decaytypes[65, 107] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[66, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 103] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 102] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 100] = new double[,] { { 18, 1.000 } };
        decaytypes[73, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 98] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 96] = new double[,] { { 1, 0.989 }, { 8, 0.011 } };
        decaytypes[77, 95] = new double[,] { { 1, 0.980 }, { 8, 0.020 } };
        decaytypes[78, 94] = new double[,] { { 1, 0.508 }, { 8, 0.492 } };
        decaytypes[79, 93] = new double[,] { { 1, 0.495 }, { 3, 0.010 }, { 8, 0.495 } };
        decaytypes[80, 92] = new double[,] { { 8, 1.000 } };
        decaytypes[66, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 104] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 103] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 102] = new double[,] { { 18, 1.000 } };
        decaytypes[72, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[73, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 98] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 97] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 96] = new double[,] { { 1, 0.930 }, { 8, 0.070 } };
        decaytypes[78, 95] = new double[,] { { 1, 0.538 }, { 8, 0.462 } };
        decaytypes[79, 94] = new double[,] { { 1, 0.065 }, { 8, 0.935 } };
        decaytypes[80, 93] = new double[,] { { 8, 1.000 } };
        decaytypes[66, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[67, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 104] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[72, 102] = new double[,] { { 8, 1.000 } };
        decaytypes[73, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 98] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 97] = new double[,] { { 1, 0.995 }, { 8, 0.005 } };
        decaytypes[78, 96] = new double[,] { { 1, 0.568 }, { 8, 0.432 } };
        decaytypes[79, 95] = new double[,] { { 1, 0.526 }, { 8, 0.474 } };
        decaytypes[80, 94] = new double[,] { { 8, 1.000 } };
        decaytypes[67, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 104] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[72, 103] = new double[,] { { 18, 1.000 } };
        decaytypes[73, 102] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 99] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 98] = new double[,] { { 1, 0.992 }, { 8, 0.008 } };
        decaytypes[78, 97] = new double[,] { { 1, 0.610 }, { 8, 0.390 } };
        decaytypes[79, 96] = new double[,] { { 1, 0.532 }, { 8, 0.468 } };
        decaytypes[80, 95] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[67, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[68, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 106] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[71, 105] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 104] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[73, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 102] = new double[,] { { 18, 1.000 } };
        decaytypes[75, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 99] = new double[,] { { 1, 0.969 }, { 8, 0.031 } };
        decaytypes[78, 98] = new double[,] { { 1, 0.714 }, { 8, 0.286 } };
        decaytypes[79, 97] = new double[,] { { 1, 0.571 }, { 8, 0.429 } };
        decaytypes[80, 96] = new double[,] { { 1, 0.526 }, { 8, 0.474 } };
        decaytypes[81, 95] = new double[,] { { 1, 0.334 }, { 3, 0.333 }, { 8, 0.333 } };
        decaytypes[68, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 106] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 105] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[73, 104] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 102] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 100] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 99] = new double[,] { { 1, 0.943 }, { 8, 0.057 } };
        decaytypes[79, 98] = new double[,] { { 1, 0.714 }, { 8, 0.286 } };
        decaytypes[80, 97] = new double[,] { { 1, 0.150 }, { 8, 0.850 } };
        decaytypes[81, 96] = new double[,] { { 3, 0.270 }, { 8, 0.730 } };
        decaytypes[68, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[69, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 107] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 106] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[73, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[74, 104] = new double[,] { { 18, 1.000 } };
        decaytypes[75, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 102] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 100] = new double[,] { { 1, 0.923 }, { 8, 0.077 } };
        decaytypes[79, 99] = new double[,] { { 1, 0.600 }, { 8, 0.400 } };
        decaytypes[80, 98] = new double[,] { { 1, 0.529 }, { 8, 0.471 } };
        decaytypes[81, 97] = new double[,] { { 1, 0.380 }, { 8, 0.620 } };
        decaytypes[82, 96] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[69, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 108] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 107] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[73, 106] = new double[,] { { 18, 1.000 } };
        decaytypes[74, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[75, 104] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 102] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 100] = new double[,] { { 1, 0.780 }, { 8, 0.220 } };
        decaytypes[80, 99] = new double[,] { { 1, 0.571 }, { 8, 0.429 } };
        decaytypes[81, 98] = new double[,] { { 1, 0.625 }, { 8, 0.375 } };
        decaytypes[82, 97] = new double[,] { { 8, 1.000 } };
        decaytypes[69, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 108] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[73, 107] = new double[,] { { 0, 0.150 }, { 18, 0.850 } };
        decaytypes[74, 106] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[75, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 104] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 102] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 101] = new double[,] { { 1, 0.982 }, { 8, 0.018 } };
        decaytypes[80, 100] = new double[,] { { 1, 0.520 }, { 8, 0.480 } };
        decaytypes[81, 99] = new double[,] { { 1, 0.940 }, { 8, 0.060 } };
        decaytypes[82, 98] = new double[,] { { 8, 1.000 } };
        decaytypes[69, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[70, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 108] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[74, 107] = new double[,] { { 18, 1.000 } };
        decaytypes[75, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 104] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 102] = new double[,] { { 1, 0.974 }, { 8, 0.026 } };
        decaytypes[80, 101] = new double[,] { { 1, 0.730 }, { 8, 0.270 } };
        decaytypes[81, 100] = new double[,] { { 1, 0.909 }, { 8, 0.091 } };
        decaytypes[82, 99] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[70, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 109] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 108] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[75, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 106] = new double[,] { { 18, 1.000 } };
        decaytypes[77, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 104] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 103] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 102] = new double[,] { { 1, 0.862 }, { 8, 0.138 } };
        decaytypes[81, 101] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 100] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[70, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 110] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 109] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[75, 108] = new double[,] { { 18, 1.000 } };
        decaytypes[76, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[77, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 104] = new double[,] { { 1, 0.995 }, { 8, 0.005 } };
        decaytypes[80, 103] = new double[,] { { 1, 0.883 }, { 8, 0.117 } };
        decaytypes[81, 102] = new double[,] { { 1, 0.980 }, { 8, 0.020 } };
        decaytypes[82, 101] = new double[,] { { 1, 0.091 }, { 8, 0.909 } };
        decaytypes[70, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 110] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[75, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[76, 108] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 104] = new double[,] { { 1, 0.989 }, { 8, 0.011 } };
        decaytypes[81, 103] = new double[,] { { 1, 0.988 }, { 8, 0.012 } };
        decaytypes[82, 102] = new double[,] { { 1, 0.556 }, { 8, 0.444 } };
        decaytypes[83, 101] = new double[,] { { 8, 1.000 } };
        decaytypes[70, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[71, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 111] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 110] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[76, 109] = new double[,] { { 18, 1.000 } };
        decaytypes[77, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 105] = new double[,] { { 1, 0.940 }, { 8, 0.060 } };
        decaytypes[81, 104] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[82, 103] = new double[,] { { 1, 0.746 }, { 8, 0.254 } };
        decaytypes[83, 102] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[71, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 112] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[75, 111] = new double[,] { { 0, 0.925 }, { 18, 0.075 } };
        decaytypes[76, 110] = new double[,] { { 8, 1.000 } };
        decaytypes[77, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 105] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 104] = new double[,] { { 1, 0.714 }, { 8, 0.286 } };
        decaytypes[83, 103] = new double[,] { { 1, 0.006 }, { 8, 0.994 } };
        decaytypes[84, 102] = new double[,] { { 3, 0.500 }, { 8, 0.500 } };
        decaytypes[71, 116] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 112] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 111] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 110] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 106] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 105] = new double[,] { { 1, 0.913 }, { 8, 0.087 } };
        decaytypes[83, 104] = new double[,] { { 8, 1.000 } };
        decaytypes[84, 103] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[71, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[72, 116] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 113] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 112] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 110] = new double[,] { { 18, 1.000 } };
        decaytypes[79, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 106] = new double[,] { { 1, 0.915 }, { 8, 0.085 } };
        decaytypes[83, 105] = new double[,] { { 1, 0.011 }, { 8, 0.989 } };
        decaytypes[84, 104] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[72, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 116] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 114] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 113] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 112] = new double[,] { { 18, 1.000 } };
        decaytypes[78, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[79, 110] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 107] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 106] = new double[,] { { 8, 1.000 } };
        decaytypes[84, 105] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[72, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[73, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 116] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 114] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[78, 112] = new double[,] { { 8, 1.000 } };
        decaytypes[79, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 110] = new double[,] { { 18, 1.000 } };
        decaytypes[81, 109] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 108] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 107] = new double[,] { { 1, 0.565 }, { 8, 0.435 } };
        decaytypes[84, 106] = new double[,] { { 8, 1.000 } };
        decaytypes[73, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 116] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 115] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 114] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[78, 113] = new double[,] { { 18, 1.000 } };
        decaytypes[79, 112] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 110] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 109] = new double[,] { { 1, 0.995 }, { 8, 0.005 } };
        decaytypes[83, 108] = new double[,] { { 1, 0.662 }, { 8, 0.338 } };
        decaytypes[84, 107] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[85, 106] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[73, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 116] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[77, 115] = new double[,] { { 0, 0.952 }, { 18, 0.048 } };
        decaytypes[78, 114] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[79, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 112] = new double[,] { { 18, 1.000 } };
        decaytypes[81, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 110] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 109] = new double[,] { { 1, 0.880 }, { 8, 0.120 } };
        decaytypes[84, 108] = new double[,] { { 1, 0.005 }, { 8, 0.995 } };
        decaytypes[85, 107] = new double[,] { { 1, 0.006 }, { 8, 0.994 } };
        decaytypes[73, 120] = new double[,] { { 0, 0.993 }, { 11, 0.007 } };
        decaytypes[74, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 116] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[78, 115] = new double[,] { { 18, 1.000 } };
        decaytypes[79, 114] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 112] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 110] = new double[,] { { 1, 0.966 }, { 8, 0.034 } };
        decaytypes[84, 109] = new double[,] { { 1, 0.048 }, { 8, 0.952 } };
        decaytypes[85, 108] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 107] = new double[,] { { 8, 1.000 } };
        decaytypes[73, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[74, 120] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 117] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 116] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[79, 115] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 114] = new double[,] { { 18, 1.000 } };
        decaytypes[81, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 112] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 111] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 110] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[85, 109] = new double[,] { { 1, 0.077 }, { 8, 0.923 } };
        decaytypes[86, 108] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[74, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 120] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 118] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 117] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[79, 116] = new double[,] { { 18, 1.000 } };
        decaytypes[80, 115] = new double[,] { { 1, 1.000 } };
        decaytypes[81, 114] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 112] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 111] = new double[,] { { 1, 0.515 }, { 8, 0.485 } };
        decaytypes[85, 110] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[86, 109] = new double[,] { { 8, 1.000 } };
        decaytypes[74, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 120] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 118] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[79, 117] = new double[,] { { 0, 0.072 }, { 1, 0.928 } };
        decaytypes[80, 116] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 115] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 114] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 113] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 112] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[85, 111] = new double[,] { { 1, 0.048 }, { 8, 0.952 } };
        decaytypes[86, 110] = new double[,] { { 8, 1.000 } };
        decaytypes[74, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[75, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 120] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 118] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[80, 117] = new double[,] { { 18, 1.000 } };
        decaytypes[81, 116] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 115] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 114] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 113] = new double[,] { { 1, 0.694 }, { 8, 0.306 } };
        decaytypes[85, 112] = new double[,] { { 1, 0.039 }, { 8, 0.961 } };
        decaytypes[86, 111] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[87, 110] = new double[,] { { 8, 1.000 } };
        decaytypes[75, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 120] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[79, 119] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 118] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 117] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 116] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 115] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 114] = new double[,] { { 1, 0.430 }, { 8, 0.570 } };
        decaytypes[85, 113] = new double[,] { { 1, 0.515 }, { 8, 0.485 } };
        decaytypes[86, 112] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[87, 111] = new double[,] { { 8, 1.000 } };
        decaytypes[75, 124] = new double[,] { { 0, 1.000 } };
        decaytypes[76, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 120] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 119] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 118] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 117] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 116] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 115] = new double[,] { { 1, 0.925 }, { 8, 0.075 } };
        decaytypes[85, 114] = new double[,] { { 1, 0.529 }, { 8, 0.471 } };
        decaytypes[86, 113] = new double[,] { { 1, 0.057 }, { 8, 0.943 } };
        decaytypes[87, 112] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[76, 124] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 121] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 120] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 119] = new double[,] { { 1, 1.000 } };
        decaytypes[82, 118] = new double[,] { { 18, 1.000 } };
        decaytypes[83, 117] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 116] = new double[,] { { 1, 0.889 }, { 8, 0.111 } };
        decaytypes[85, 115] = new double[,] { { 1, 0.480 }, { 8, 0.520 } };
        decaytypes[86, 114] = new double[,] { { 1, 0.521 }, { 8, 0.479 } };
        decaytypes[87, 113] = new double[,] { { 0, 0.010 }, { 8, 0.976 }, { 20, 0.014 } };
        decaytypes[76, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 124] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 122] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 121] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 120] = new double[,] { { 18, 1.000 } };
        decaytypes[82, 119] = new double[,] { { 1, 1.000 } };
        decaytypes[83, 118] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 117] = new double[,] { { 1, 0.989 }, { 8, 0.011 } };
        decaytypes[85, 116] = new double[,] { { 1, 0.290 }, { 8, 0.710 } };
        decaytypes[86, 115] = new double[,] { { 1, 0.329 }, { 8, 0.671 } };
        decaytypes[87, 114] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 113] = new double[,] { { 8, 1.000 } };
        decaytypes[76, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[77, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 124] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 122] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 121] = new double[,] { { 18, 1.000 } };
        decaytypes[82, 120] = new double[,] { { 18, 1.000 } };
        decaytypes[83, 119] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 118] = new double[,] { { 1, 0.981 }, { 8, 0.019 } };
        decaytypes[85, 117] = new double[,] { { 1, 0.893 }, { 8, 0.107 } };
        decaytypes[86, 116] = new double[,] { { 1, 0.562 }, { 8, 0.438 } };
        decaytypes[87, 115] = new double[,] { { 1, 0.123 }, { 8, 0.877 } };
        decaytypes[88, 114] = new double[,] { { 8, 1.000 } };
        decaytypes[76, 127] = new double[,] { { 0, 0.930 }, { 11, 0.070 } };
        decaytypes[77, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 124] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 123] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 122] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[82, 121] = new double[,] { { 18, 1.000 } };
        decaytypes[83, 120] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 119] = new double[,] { { 1, 1.000 } };
        decaytypes[85, 118] = new double[,] { { 1, 0.690 }, { 8, 0.310 } };
        decaytypes[86, 117] = new double[,] { { 1, 0.602 }, { 8, 0.398 } };
        decaytypes[87, 116] = new double[,] { { 1, 0.048 }, { 8, 0.952 } };
        decaytypes[88, 115] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[77, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[78, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 124] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[81, 123] = new double[,] { { 0, 0.971 }, { 18, 0.029 } };
        decaytypes[82, 122] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[83, 121] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 120] = new double[,] { { 1, 0.993 }, { 8, 0.007 } };
        decaytypes[85, 119] = new double[,] { { 1, 0.962 }, { 8, 0.038 } };
        decaytypes[86, 118] = new double[,] { { 1, 0.580 }, { 8, 0.420 } };
        decaytypes[87, 117] = new double[,] { { 1, 0.510 }, { 8, 0.490 } };
        decaytypes[88, 116] = new double[,] { { 8, 1.000 } };
        decaytypes[77, 128] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[78, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 124] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[82, 123] = new double[,] { { 18, 1.000 } };
        decaytypes[83, 122] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 121] = new double[,] { { 1, 1.000 } };
        decaytypes[85, 120] = new double[,] { { 1, 0.909 }, { 8, 0.091 } };
        decaytypes[86, 119] = new double[,] { { 1, 0.803 }, { 8, 0.197 } };
        decaytypes[87, 118] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[88, 117] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[89, 116] = new double[,] { { 8, 1.000 } };
        decaytypes[78, 128] = new double[,] { { 0, 1.000 } };
        decaytypes[79, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 125] = new double[,] { { 0, 1.000 } };
        decaytypes[82, 124] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[83, 123] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 122] = new double[,] { { 1, 0.946 }, { 8, 0.054 } };
        decaytypes[85, 121] = new double[,] { { 1, 0.991 }, { 8, 0.009 } };
        decaytypes[86, 120] = new double[,] { { 1, 0.380 }, { 8, 0.620 } };
        decaytypes[87, 119] = new double[,] { { 1, 0.531 }, { 8, 0.469 } };
        decaytypes[88, 118] = new double[,] { { 1, 0.024 }, { 8, 0.976 } };
        decaytypes[89, 117] = new double[,] { { 8, 1.000 } };
        decaytypes[78, 129] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[79, 128] = new double[,] { { 0, 1.000 } };
        decaytypes[80, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 126] = new double[,] { { 0, 1.000 } };
        decaytypes[82, 125] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[83, 124] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 123] = new double[,] { { 1, 1.000 } };
        decaytypes[85, 122] = new double[,] { { 1, 0.909 }, { 8, 0.091 } };
        decaytypes[86, 121] = new double[,] { { 1, 0.790 }, { 8, 0.210 } };
        decaytypes[87, 120] = new double[,] { { 1, 0.513 }, { 8, 0.487 } };
        decaytypes[88, 119] = new double[,] { { 1, 0.538 }, { 8, 0.462 } };
        decaytypes[89, 118] = new double[,] { { 8, 1.000 } };
        decaytypes[78, 130] = new double[,] { { 0, 0.100 }, { 11, 0.900 } };
        decaytypes[79, 129] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[80, 128] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[82, 126] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[83, 125] = new double[,] { { 1, 1.000 } };
        decaytypes[84, 124] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 123] = new double[,] { { 1, 0.995 }, { 8, 0.005 } };
        decaytypes[86, 122] = new double[,] { { 1, 0.380 }, { 8, 0.620 } };
        decaytypes[87, 121] = new double[,] { { 1, 0.110 }, { 8, 0.890 } };
        decaytypes[88, 120] = new double[,] { { 1, 0.535 }, { 8, 0.465 } };
        decaytypes[89, 119] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[90, 118] = new double[,] { { 8, 1.000 } };
        decaytypes[79, 130] = new double[,] { { 0, 0.100 }, { 11, 0.900 } };
        decaytypes[80, 129] = new double[,] { { 0, 1.000 } };
        decaytypes[81, 128] = new double[,] { { 0, 1.000 } };
        decaytypes[82, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 126] = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        decaytypes[84, 125] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 124] = new double[,] { { 1, 0.959 }, { 8, 0.041 } };
        decaytypes[86, 123] = new double[,] { { 1, 0.830 }, { 8, 0.170 } };
        decaytypes[87, 122] = new double[,] { { 1, 0.110 }, { 8, 0.890 } };
        decaytypes[88, 121] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[89, 120] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[90, 119] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[79, 131] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[80, 130] = new double[,] { { 0, 0.978 }, { 11, 0.022 } };
        decaytypes[81, 129] = new double[,] { { 0, 1.000 } };
        decaytypes[82, 128] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 127] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 125] = new double[,] { { 1, 1.000 } };
        decaytypes[86, 124] = new double[,] { { 1, 0.510 }, { 8, 0.490 } };
        decaytypes[87, 123] = new double[,] { { 1, 0.585 }, { 8, 0.415 } };
        decaytypes[88, 122] = new double[,] { { 1, 0.038 }, { 8, 0.962 } };
        decaytypes[89, 121] = new double[,] { { 1, 0.083 }, { 8, 0.917 } };
        decaytypes[90, 120] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[80, 131] = new double[,] { { 0, 0.937 }, { 11, 0.063 } };
        decaytypes[81, 130] = new double[,] { { 0, 0.978 }, { 11, 0.022 } };
        decaytypes[82, 129] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[84, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 126] = new double[,] { { 8, 0.418 }, { 18, 0.582 } };
        decaytypes[86, 125] = new double[,] { { 1, 0.726 }, { 8, 0.274 } };
        decaytypes[87, 124] = new double[,] { { 1, 0.535 }, { 8, 0.465 } };
        decaytypes[88, 123] = new double[,] { { 1, 0.070 }, { 8, 0.930 } };
        decaytypes[89, 122] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 121] = new double[,] { { 1, 0.005 }, { 8, 0.995 } };
        decaytypes[91, 120] = new double[,] { { 1, 0.334 }, { 3, 0.333 }, { 8, 0.333 } };
        decaytypes[80, 132] = new double[,] { { 0, 0.920 }, { 11, 0.080 } };
        decaytypes[81, 131] = new double[,] { { 0, 0.982 }, { 11, 0.018 } };
        decaytypes[82, 130] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 129] = new double[,] { { 0, 0.641 }, { 8, 0.359 } };
        decaytypes[84, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 125] = new double[,] { { 1, 0.570 }, { 8, 0.430 } };
        decaytypes[88, 124] = new double[,] { { 1, 0.130 }, { 8, 0.870 } };
        decaytypes[89, 123] = new double[,] { { 1, 0.029 }, { 8, 0.971 } };
        decaytypes[90, 122] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 121] = new double[,] { { 8, 1.000 } };
        decaytypes[80, 133] = new double[,] { { 0, 0.700 }, { 11, 0.300 } };
        decaytypes[81, 132] = new double[,] { { 0, 0.924 }, { 11, 0.076 } };
        decaytypes[82, 131] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 130] = new double[,] { { 0, 0.979 }, { 8, 0.021 } };
        decaytypes[84, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 126] = new double[,] { { 1, 0.006 }, { 8, 0.994 } };
        decaytypes[88, 125] = new double[,] { { 1, 0.556 }, { 8, 0.444 } };
        decaytypes[89, 124] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[90, 123] = new double[,] { { 1, 0.014 }, { 8, 0.986 } };
        decaytypes[91, 122] = new double[,] { { 8, 1.000 } };
        decaytypes[80, 134] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[81, 133] = new double[,] { { 0, 0.660 }, { 11, 0.340 } };
        decaytypes[82, 132] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 131] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 125] = new double[,] { { 1, 0.110 }, { 8, 0.890 } };
        decaytypes[90, 124] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 123] = new double[,] { { 8, 1.000 } };
        decaytypes[80, 135] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[81, 134] = new double[,] { { 0, 0.954 }, { 11, 0.046 } };
        decaytypes[82, 133] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 132] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 125] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 124] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 123] = new double[,] { { 1, 1.000 } };
        decaytypes[80, 136] = new double[,] { { 0, 0.940 }, { 11, 0.060 } };
        decaytypes[81, 135] = new double[,] { { 0, 0.885 }, { 11, 0.115 } };
        decaytypes[82, 134] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 133] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 125] = new double[,] { { 1, 0.020 }, { 8, 0.980 } };
        decaytypes[92, 124] = new double[,] { { 8, 1.000 } };
        decaytypes[81, 136] = new double[,] { { 11, 1.000 } };
        decaytypes[82, 135] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 134] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 133] = new double[,] { { 0, 0.050 }, { 8, 0.950 } };
        decaytypes[85, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 125] = new double[,] { { 8, 1.000 } };
        decaytypes[81, 137] = new double[,] { { 0, 0.300 }, { 11, 0.700 } };
        decaytypes[82, 136] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 135] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[86, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[82, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 136] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 135] = new double[,] { { 0, 0.780 }, { 8, 0.220 } };
        decaytypes[85, 134] = new double[,] { { 0, 0.517 }, { 8, 0.483 } };
        decaytypes[86, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 126] = new double[,] { { 8, 1.000 } };
        decaytypes[82, 138] = new double[,] { { 0, 1.000 } };
        decaytypes[83, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[84, 136] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 135] = new double[,] { { 0, 0.920 }, { 8, 0.080 } };
        decaytypes[86, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 128] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[93, 127] = new double[,] { { 8, 1.000 } };
        decaytypes[83, 138] = new double[,] { { 0, 0.980 }, { 11, 0.020 } };
        decaytypes[84, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 136] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 135] = new double[,] { { 0, 0.780 }, { 8, 0.220 } };
        decaytypes[87, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[88, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 129] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[93, 128] = new double[,] { { 8, 1.000 } };
        decaytypes[83, 139] = new double[,] { { 0, 0.990 }, { 11, 0.010 } };
        decaytypes[84, 138] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 136] = new double[,] { { 8, 1.000 } };
        decaytypes[87, 135] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 133] = new double[,] { { 1, 0.010 }, { 8, 0.990 } };
        decaytypes[90, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 129] = new double[,] { { 8, 1.000 } };
        decaytypes[83, 140] = new double[,] { { 0, 0.950 }, { 11, 0.050 } };
        decaytypes[84, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 138] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 136] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 135] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 134] = new double[,] { { 8, 0.990 }, { 18, 0.010 } };
        decaytypes[90, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 130] = new double[,] { { 8, 1.000 } };
        decaytypes[83, 141] = new double[,] { { 0, 0.900 }, { 11, 0.100 } };
        decaytypes[84, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 138] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 136] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 135] = new double[,] { { 0, 0.015 }, { 1, 0.892 }, { 8, 0.093 } };
        decaytypes[90, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 132] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 131] = new double[,] { { 8, 1.000 } };
        decaytypes[84, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 138] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 137] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 136] = new double[,] { { 8, 1.000 } };
        decaytypes[90, 135] = new double[,] { { 8, 0.900 }, { 18, 0.100 } };
        decaytypes[91, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 132] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[84, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 138] = new double[,] { { 8, 1.000 } };
        decaytypes[89, 137] = new double[,] { { 0, 0.830 }, { 18, 0.170 } };
        decaytypes[90, 136] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 135] = new double[,] { { 1, 0.260 }, { 8, 0.740 } };
        decaytypes[92, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[84, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[85, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[86, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 138] = new double[,] { { 0, 0.986 }, { 8, 0.014 } };
        decaytypes[90, 137] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 136] = new double[,] { { 8, 0.850 }, { 18, 0.150 } };
        decaytypes[92, 135] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 134] = new double[,] { { 8, 1.000 } };
        decaytypes[94, 133] = new double[,] { { 8, 1.000 } };
        decaytypes[85, 143] = new double[,] { { 0, 0.994 }, { 11, 0.006 } };
        decaytypes[86, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 139] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 138] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 137] = new double[,] { { 1, 0.982 }, { 8, 0.018 } };
        decaytypes[92, 136] = new double[,] { { 8, 0.950 }, { 18, 0.050 } };
        decaytypes[93, 135] = new double[,] { { 8, 0.400 }, { 18, 0.600 } };
        decaytypes[94, 134] = new double[,] { { 1, 0.065 }, { 8, 0.935 } };
        decaytypes[85, 144] = new double[,] { { 0, 0.960 }, { 11, 0.040 } };
        decaytypes[86, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 140] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 139] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 138] = new double[,] { { 18, 1.000 } };
        decaytypes[92, 137] = new double[,] { { 1, 0.800 }, { 8, 0.200 } };
        decaytypes[93, 136] = new double[,] { { 1, 0.595 }, { 8, 0.405 } };
        decaytypes[94, 135] = new double[,] { { 1, 0.468 }, { 8, 0.467 }, { 19, 0.065 } };
        decaytypes[95, 134] = new double[,] { { 1, 0.524 }, { 8, 0.476 } };
        decaytypes[86, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 140] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 139] = new double[,] { { 0, 0.078 }, { 1, 0.922 } };
        decaytypes[92, 138] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 137] = new double[,] { { 1, 0.970 }, { 8, 0.030 } };
        decaytypes[94, 136] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[95, 135] = new double[,] { { 19, 0.500 }, { 21, 0.500 } };
        decaytypes[86, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[87, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 140] = new double[,] { { 8, 1.000 } };
        decaytypes[92, 139] = new double[,] { { 18, 1.000 } };
        decaytypes[93, 138] = new double[,] { { 1, 0.980 }, { 8, 0.020 } };
        decaytypes[94, 137] = new double[,] { { 1, 0.870 }, { 8, 0.130 } };
        decaytypes[95, 136] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[96, 135] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[87, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 142] = new double[,] { { 8, 1.000 } };
        decaytypes[91, 141] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 140] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 139] = new double[,] { { 1, 1.000 } };
        decaytypes[94, 138] = new double[,] { { 8, 0.109 }, { 18, 0.891 } };
        decaytypes[95, 137] = new double[,] { { 1, 0.971 }, { 8, 0.029 } };
        decaytypes[96, 136] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[87, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[88, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 142] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 141] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 140] = new double[,] { { 1, 1.000 } };
        decaytypes[94, 139] = new double[,] { { 1, 1.000 } };
        decaytypes[95, 138] = new double[,] { { 1, 0.957 }, { 8, 0.043 } };
        decaytypes[96, 137] = new double[,] { { 1, 0.800 }, { 8, 0.200 } };
        decaytypes[97, 136] = new double[,] { { 1, 0.549 }, { 8, 0.451 } };
        decaytypes[88, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 143] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 142] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 141] = new double[,] { { 1, 1.000 } };
        decaytypes[94, 140] = new double[,] { { 8, 0.060 }, { 18, 0.940 } };
        decaytypes[95, 139] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 138] = new double[,] { { 1, 0.710 }, { 8, 0.270 }, { 19, 0.020 } };
        decaytypes[97, 137] = new double[,] { { 1, 0.200 }, { 8, 0.800 } };
        decaytypes[88, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[89, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 144] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 143] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 142] = new double[,] { { 18, 1.000 } };
        decaytypes[94, 141] = new double[,] { { 1, 1.000 } };
        decaytypes[95, 140] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 139] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[97, 138] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[89, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 144] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 143] = new double[,] { { 0, 0.135 }, { 18, 0.865 } };
        decaytypes[94, 142] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 141] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 140] = new double[,] { { 1, 0.820 }, { 8, 0.180 } };
        decaytypes[97, 139] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[89, 148] = new double[,] { { 0, 1.000 } };
        decaytypes[90, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 144] = new double[,] { { 8, 1.000 } };
        decaytypes[94, 143] = new double[,] { { 18, 1.000 } };
        decaytypes[95, 142] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 141] = new double[,] { { 1, 0.982 }, { 8, 0.018 } };
        decaytypes[97, 140] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[98, 139] = new double[,] { { 1, 0.500 }, { 8, 0.350 }, { 19, 0.150 } };
        decaytypes[90, 148] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 146] = new double[,] { { 8, 1.000 } };
        decaytypes[93, 145] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 144] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 143] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 142] = new double[,] { { 8, 0.037 }, { 18, 0.963 } };
        decaytypes[97, 141] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[98, 140] = new double[,] { { 1, 0.500 }, { 19, 0.500 } };
        decaytypes[90, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[91, 148] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 146] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 145] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 144] = new double[,] { { 18, 1.000 } };
        decaytypes[96, 143] = new double[,] { { 1, 1.000 } };
        decaytypes[97, 142] = new double[,] { { 1, 0.980 }, { 8, 0.010 }, { 19, 0.010 } };
        decaytypes[98, 141] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[99, 140] = new double[,] { { 1, 0.334 }, { 8, 0.333 }, { 19, 0.333 } };
        decaytypes[91, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 148] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 146] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 145] = new double[,] { { 1, 1.000 } };
        decaytypes[96, 144] = new double[,] { { 8, 0.995 }, { 18, 0.005 } };
        decaytypes[97, 143] = new double[,] { { 1, 0.909 }, { 8, 0.091 } };
        decaytypes[98, 142] = new double[,] { { 8, 0.985 }, { 19, 0.015 } };
        decaytypes[99, 141] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[91, 150] = new double[,] { { 0, 1.000 } };
        decaytypes[92, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 148] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 147] = new double[,] { { 0, 1.000 } };
        decaytypes[95, 146] = new double[,] { { 8, 1.000 } };
        decaytypes[96, 145] = new double[,] { { 8, 0.010 }, { 18, 0.990 } };
        decaytypes[97, 144] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[98, 143] = new double[,] { { 1, 0.800 }, { 8, 0.200 } };
        decaytypes[99, 142] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[100, 141] = new double[,] { { 1, 0.095 }, { 8, 0.111 }, { 19, 0.794 } };
        decaytypes[92, 150] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 148] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 147] = new double[,] { { 0, 0.827 }, { 18, 0.173 } };
        decaytypes[96, 146] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 145] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[98, 144] = new double[,] { { 1, 0.556 }, { 8, 0.444 } };
        decaytypes[99, 143] = new double[,] { { 1, 0.424 }, { 8, 0.570 }, { 21, 0.006 } };
        decaytypes[100, 142] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[92, 151] = new double[,] { { 0, 1.000 } };
        decaytypes[93, 150] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[95, 148] = new double[,] { { 8, 1.000 } };
        decaytypes[96, 147] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 146] = new double[,] { { 1, 1.000 } };
        decaytypes[98, 145] = new double[,] { { 1, 0.860 }, { 8, 0.140 } };
        decaytypes[99, 144] = new double[,] { { 1, 0.386 }, { 8, 0.604 }, { 19, 0.010 } };
        decaytypes[100, 143] = new double[,] { { 1, 0.500 }, { 8, 0.455 }, { 19, 0.045 } };
        decaytypes[93, 151] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 150] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 149] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 148] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 147] = new double[,] { { 1, 1.000 } };
        decaytypes[98, 146] = new double[,] { { 8, 0.500 }, { 18, 0.500 } };
        decaytypes[99, 145] = new double[,] { { 1, 0.952 }, { 8, 0.048 } };
        decaytypes[100, 144] = new double[,] { { 1, 0.020 }, { 19, 0.980 } };
        decaytypes[93, 152] = new double[,] { { 0, 1.000 } };
        decaytypes[94, 151] = new double[,] { { 0, 1.000 } };
        decaytypes[95, 150] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 149] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 148] = new double[,] { { 18, 1.000 } };
        decaytypes[98, 147] = new double[,] { { 1, 0.735 }, { 8, 0.265 } };
        decaytypes[99, 146] = new double[,] { { 1, 0.714 }, { 8, 0.286 } };
        decaytypes[100, 145] = new double[,] { { 1, 0.040 }, { 8, 0.960 } };
        decaytypes[101, 144] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[94, 152] = new double[,] { { 0, 1.000 } };
        decaytypes[95, 151] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 150] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 149] = new double[,] { { 1, 1.000 } };
        decaytypes[98, 148] = new double[,] { { 8, 1.000 } };
        decaytypes[99, 147] = new double[,] { { 1, 0.901 }, { 8, 0.099 } };
        decaytypes[100, 146] = new double[,] { { 8, 0.875 }, { 18, 0.012 }, { 19, 0.053 }, { 21, 0.060 } };
        decaytypes[101, 145] = new double[,] { { 8, 1.000 } };
        decaytypes[94, 153] = new double[,] { { 0, 1.000 } };
        decaytypes[95, 152] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 151] = new double[,] { { 8, 1.000 } };
        decaytypes[97, 150] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[98, 149] = new double[,] { { 18, 1.000 } };
        decaytypes[99, 148] = new double[,] { { 1, 0.930 }, { 8, 0.070 } };
        decaytypes[100, 147] = new double[,] { { 1, 0.610 }, { 8, 0.390 } };
        decaytypes[101, 146] = new double[,] { { 8, 1.000 } };
        decaytypes[95, 153] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 152] = new double[,] { { 8, 0.916 }, { 19, 0.084 } };
        decaytypes[97, 151] = new double[,] { { 8, 1.000 } };
        decaytypes[98, 150] = new double[,] { { 8, 1.000 } };
        decaytypes[99, 149] = new double[,] { { 1, 1.000 } };
        decaytypes[100, 148] = new double[,] { { 1, 0.050 }, { 8, 0.950 } };
        decaytypes[101, 147] = new double[,] { { 1, 0.800 }, { 8, 0.200 } };
        decaytypes[102, 146] = new double[,] { { 19, 1.000 } };
        decaytypes[95, 154] = new double[,] { { 0, 1.000 } };
        decaytypes[96, 153] = new double[,] { { 0, 1.000 } };
        decaytypes[97, 152] = new double[,] { { 0, 1.000 } };
        decaytypes[98, 151] = new double[,] { { 8, 1.000 } };
        decaytypes[99, 150] = new double[,] { { 1, 0.994 }, { 8, 0.006 } };
        decaytypes[100, 149] = new double[,] { { 1, 0.752 }, { 8, 0.248 } };
        decaytypes[101, 148] = new double[,] { { 1, 0.625 }, { 8, 0.375 } };
        decaytypes[102, 147] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[96, 154] = new double[,] { { 0, 0.080 }, { 8, 0.180 }, { 19, 0.740 } };
        decaytypes[97, 153] = new double[,] { { 0, 1.000 } };
        decaytypes[98, 152] = new double[,] { { 8, 1.000 } };
        decaytypes[99, 151] = new double[,] { { 1, 0.492 }, { 8, 0.508 } };
        decaytypes[100, 150] = new double[,] { { 8, 0.900 }, { 18, 0.100 } };
        decaytypes[101, 149] = new double[,] { { 1, 0.930 }, { 8, 0.070 } };
        decaytypes[102, 148] = new double[,] { { 8, 0.021 }, { 19, 0.979 } };
        decaytypes[96, 155] = new double[,] { { 0, 1.000 } };
        decaytypes[97, 154] = new double[,] { { 0, 1.000 } };
        decaytypes[98, 153] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[99, 152] = new double[,] { { 8, 0.005 }, { 18, 0.995 } };
        decaytypes[100, 151] = new double[,] { { 1, 0.982 }, { 8, 0.018 } };
        decaytypes[101, 150] = new double[,] { { 1, 0.909 }, { 8, 0.091 } };
        decaytypes[102, 149] = new double[,] { { 1, 0.546 }, { 8, 0.454 } };
        decaytypes[103, 148] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[96, 156] = new double[,] { { 0, 1.000 } };
        decaytypes[97, 155] = new double[,] { { 0, 0.500 }, { 8, 0.500 } };
        decaytypes[98, 154] = new double[,] { { 8, 0.969 }, { 19, 0.031 } };
        decaytypes[99, 153] = new double[,] { { 8, 0.780 }, { 18, 0.220 } };
        decaytypes[100, 152] = new double[,] { { 8, 1.000 } };
        decaytypes[101, 151] = new double[,] { { 1, 0.333 }, { 8, 0.667 } };
        decaytypes[102, 150] = new double[,] { { 1, 0.011 }, { 8, 0.667 }, { 19, 0.322 } };
        decaytypes[103, 149] = new double[,] { { 1, 0.413 }, { 8, 0.581 }, { 19, 0.006 } };
        decaytypes[97, 156] = new double[,] { { 0, 1.000 } };
        decaytypes[98, 155] = new double[,] { { 0, 1.000 } };
        decaytypes[99, 154] = new double[,] { { 8, 1.000 } };
        decaytypes[100, 153] = new double[,] { { 8, 0.120 }, { 18, 0.880 } };
        decaytypes[101, 152] = new double[,] { { 1, 0.994 }, { 8, 0.006 } };
        decaytypes[102, 151] = new double[,] { { 1, 0.645 }, { 8, 0.355 } };
        decaytypes[103, 150] = new double[,] { { 1, 0.010 }, { 8, 0.962 }, { 19, 0.028 } };
        decaytypes[104, 149] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[97, 157] = new double[,] { { 0, 1.000 } };
        decaytypes[98, 156] = new double[,] { { 19, 1.000 } };
        decaytypes[99, 155] = new double[,] { { 8, 1.000 } };
        decaytypes[100, 154] = new double[,] { { 8, 1.000 } };
        decaytypes[101, 153] = new double[,] { { 1, 0.500 }, { 8, 0.500 } };
        decaytypes[102, 152] = new double[,] { { 1, 0.100 }, { 8, 0.900 } };
        decaytypes[103, 151] = new double[,] { { 1, 0.140 }, { 8, 0.360 }, { 19, 0.500 } };
        decaytypes[104, 150] = new double[,] { { 8, 0.015 }, { 19, 0.985 } };
        decaytypes[98, 157] = new double[,] { { 0, 1.000 } };
        decaytypes[99, 156] = new double[,] { { 0, 0.920 }, { 8, 0.080 } };
        decaytypes[100, 155] = new double[,] { { 8, 1.000 } };
        decaytypes[101, 154] = new double[,] { { 1, 0.930 }, { 8, 0.070 } };
        decaytypes[102, 153] = new double[,] { { 1, 0.700 }, { 8, 0.300 } };
        decaytypes[103, 152] = new double[,] { { 8, 1.000 } };
        decaytypes[104, 151] = new double[,] { { 1, 0.010 }, { 8, 0.511 }, { 19, 0.479 } };
        decaytypes[105, 150] = new double[,] { { 8, 0.833 }, { 19, 0.167 } };
        decaytypes[98, 158] = new double[,] { { 19, 1.000 } };
        decaytypes[99, 157] = new double[,] { { 0, 1.000 } };
        decaytypes[100, 156] = new double[,] { { 8, 0.081 }, { 19, 0.919 } };
        decaytypes[101, 155] = new double[,] { { 1, 0.334 }, { 8, 0.333 }, { 19, 0.333 } };
        decaytypes[102, 154] = new double[,] { { 8, 0.995 }, { 19, 0.005 } };
        decaytypes[103, 153] = new double[,] { { 1, 0.150 }, { 8, 0.850 } };
        decaytypes[104, 152] = new double[,] { { 19, 1.000 } };
        decaytypes[105, 151] = new double[,] { { 1, 0.175 }, { 8, 0.340 }, { 19, 0.485 } };
        decaytypes[99, 158] = new double[,] { { 0, 1.000 } };
        decaytypes[100, 157] = new double[,] { { 8, 1.000 } };
        decaytypes[101, 156] = new double[,] { { 8, 0.148 }, { 18, 0.842 }, { 19, 0.010 } };
        decaytypes[102, 155] = new double[,] { { 1, 0.130 }, { 8, 0.870 } };
        decaytypes[103, 154] = new double[,] { { 8, 1.000 } };
        decaytypes[104, 153] = new double[,] { { 1, 0.160 }, { 8, 0.829 }, { 19, 0.011 } };
        decaytypes[105, 152] = new double[,] { { 1, 0.010 }, { 8, 0.931 }, { 19, 0.059 } };
        decaytypes[99, 159] = new double[,] { { 0, 0.500 }, { 8, 0.500 } };
        decaytypes[100, 158] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[101, 157] = new double[,] { { 8, 1.000 } };
        decaytypes[102, 156] = new double[,] { { 19, 1.000 } };
        decaytypes[103, 155] = new double[,] { { 1, 0.025 }, { 8, 0.975 } };
        decaytypes[104, 154] = new double[,] { { 8, 0.130 }, { 19, 0.870 } };
        decaytypes[105, 153] = new double[,] { { 1, 0.366 }, { 8, 0.624 }, { 19, 0.010 } };
        decaytypes[106, 152] = new double[,] { { 8, 0.167 }, { 19, 0.833 } };
        decaytypes[100, 159] = new double[,] { { 19, 1.000 } };
        decaytypes[101, 158] = new double[,] { { 8, 0.013 }, { 19, 0.987 } };
        decaytypes[102, 157] = new double[,] { { 8, 0.682 }, { 18, 0.227 }, { 19, 0.091 } };
        decaytypes[103, 156] = new double[,] { { 1, 0.006 }, { 8, 0.775 }, { 19, 0.219 } };
        decaytypes[104, 155] = new double[,] { { 8, 0.920 }, { 19, 0.080 } };
        decaytypes[105, 154] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 153] = new double[,] { { 8, 0.960 }, { 18, 0.010 }, { 19, 0.030 } };
        decaytypes[100, 160] = new double[,] { { 19, 1.000 } };
        decaytypes[101, 159] = new double[,] { { 0, 0.031 }, { 8, 0.044 }, { 18, 0.044 }, { 19, 0.881 } };
        decaytypes[102, 158] = new double[,] { { 19, 1.000 } };
        decaytypes[103, 157] = new double[,] { { 1, 0.200 }, { 8, 0.800 } };
        decaytypes[104, 156] = new double[,] { { 8, 0.020 }, { 19, 0.980 } };
        decaytypes[105, 155] = new double[,] { { 1, 0.024 }, { 8, 0.882 }, { 19, 0.094 } };
        decaytypes[106, 154] = new double[,] { { 8, 0.400 }, { 19, 0.600 } };
        decaytypes[107, 153] = new double[,] { { 1, 0.334 }, { 8, 0.333 }, { 19, 0.333 } };
        decaytypes[101, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[102, 159] = new double[,] { { 8, 1.000 } };
        decaytypes[103, 158] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[104, 157] = new double[,] { { 8, 0.270 }, { 19, 0.730 } };
        decaytypes[105, 156] = new double[,] { { 8, 0.578 }, { 19, 0.422 } };
        decaytypes[106, 155] = new double[,] { { 1, 0.013 }, { 8, 0.981 }, { 19, 0.006 } };
        decaytypes[107, 154] = new double[,] { { 8, 0.950 }, { 19, 0.050 } };
        decaytypes[101, 161] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[102, 160] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[103, 159] = new double[,] { { 1, 0.476 }, { 8, 0.476 }, { 19, 0.048 } };
        decaytypes[104, 158] = new double[,] { { 19, 1.000 } };
        decaytypes[105, 157] = new double[,] { { 1, 0.020 }, { 8, 0.645 }, { 19, 0.335 } };
        decaytypes[106, 156] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 155] = new double[,] { { 8, 0.833 }, { 19, 0.167 } };
        decaytypes[102, 161] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[103, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[104, 159] = new double[,] { { 8, 0.231 }, { 19, 0.769 } };
        decaytypes[105, 158] = new double[,] { { 1, 0.042 }, { 8, 0.614 }, { 19, 0.344 } };
        decaytypes[106, 157] = new double[,] { { 8, 0.870 }, { 19, 0.130 } };
        decaytypes[107, 156] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 155] = new double[,] { { 8, 0.923 }, { 19, 0.077 } };
        decaytypes[102, 162] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[103, 161] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[104, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[105, 159] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 158] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 157] = new double[,] { { 1, 0.500 }, { 8, 0.430 }, { 19, 0.070 } };
        decaytypes[108, 156] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[103, 162] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[104, 161] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[105, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 159] = new double[,] { { 8, 0.333 }, { 19, 0.667 } };
        decaytypes[107, 158] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 157] = new double[,] { { 8, 0.990 }, { 19, 0.010 } };
        decaytypes[109, 156] = new double[,] { { 8, 1.000 } };
        decaytypes[103, 163] = new double[,] { { 19, 1.000 } };
        decaytypes[104, 162] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[105, 161] = new double[,] { { 1, 0.334 }, { 8, 0.333 }, { 19, 0.333 } };
        decaytypes[106, 160] = new double[,] { { 19, 1.000 } };
        decaytypes[107, 159] = new double[,] { { 1, 0.334 }, { 8, 0.333 }, { 19, 0.333 } };
        decaytypes[108, 158] = new double[,] { { 8, 0.986 }, { 19, 0.014 } };
        decaytypes[109, 157] = new double[,] { { 8, 0.948 }, { 19, 0.052 } };
        decaytypes[104, 163] = new double[,] { { 19, 1.000 } };
        decaytypes[105, 162] = new double[,] { { 19, 1.000 } };
        decaytypes[106, 161] = new double[,] { { 8, 0.170 }, { 19, 0.830 } };
        decaytypes[107, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 159] = new double[,] { { 8, 0.444 }, { 19, 0.556 } };
        decaytypes[109, 158] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 157] = new double[,] { { 8, 1.000 } };
        decaytypes[104, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[105, 163] = new double[,] { { 1, 0.500 }, { 19, 0.500 } };
        decaytypes[106, 162] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 161] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[108, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[109, 159] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 158] = new double[,] { { 8, 1.000 } };
        decaytypes[105, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[106, 163] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 162] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 161] = new double[,] { { 8, 1.000 } };
        decaytypes[109, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 159] = new double[,] { { 8, 1.000 } };
        decaytypes[105, 165] = new double[,] { { 19, 1.000 } };
        decaytypes[106, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 163] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 162] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[109, 161] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 160] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 165] = new double[,] { { 8, 0.700 }, { 19, 0.300 } };
        decaytypes[107, 164] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 163] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 161] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[107, 165] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[109, 163] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 162] = new double[,] { { 19, 1.000 } };
        decaytypes[111, 161] = new double[,] { { 8, 1.000 } };
        decaytypes[106, 167] = new double[,] { { 19, 1.000 } };
        decaytypes[107, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[108, 165] = new double[,] { { 8, 1.000 } };
        decaytypes[109, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 163] = new double[,] { { 8, 1.000 } };
        decaytypes[111, 162] = new double[,] { { 8, 1.000 } };
        decaytypes[107, 167] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[109, 165] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 163] = new double[,] { { 8, 1.000 } };
        decaytypes[107, 168] = new double[,] { { 19, 1.000 } };
        decaytypes[108, 167] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 165] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 164] = new double[,] { { 8, 1.000 } };
        decaytypes[108, 168] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[109, 167] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 165] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[112, 164] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[108, 169] = new double[,] { { 19, 1.000 } };
        decaytypes[109, 168] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 167] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[112, 165] = new double[,] { { 8, 1.000 } };
        decaytypes[109, 169] = new double[,] { { 8, 1.000 } };
        decaytypes[110, 168] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 167] = new double[,] { { 8, 1.000 } };
        decaytypes[112, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[113, 165] = new double[,] { { 8, 1.000 } };
        decaytypes[109, 170] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 169] = new double[,] { { 8, 0.100 }, { 19, 0.900 } };
        decaytypes[112, 167] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[113, 166] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 170] = new double[,] { { 19, 1.000 } };
        decaytypes[111, 169] = new double[,] { { 8, 1.000 } };
        decaytypes[112, 168] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[113, 167] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[110, 171] = new double[,] { { 8, 0.150 }, { 19, 0.850 } };
        decaytypes[112, 169] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[113, 168] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[111, 171] = new double[,] { { 8, 1.000 } };
        decaytypes[112, 170] = new double[,] { { 19, 1.000 } };
        decaytypes[113, 169] = new double[,] { { 8, 1.000 } };
        decaytypes[111, 172] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[112, 171] = new double[,] { { 8, 0.909 }, { 19, 0.091 } };
        decaytypes[112, 172] = new double[,] { { 19, 1.000 } };
        decaytypes[113, 171] = new double[,] { { 8, 1.000 } };
        decaytypes[114, 170] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[112, 173] = new double[,] { { 8, 1.000 } };
        decaytypes[114, 171] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[113, 173] = new double[,] { { 8, 1.000 } };
        decaytypes[114, 172] = new double[,] { { 8, 0.400 }, { 19, 0.600 } };
        decaytypes[114, 173] = new double[,] { { 8, 1.000 } };
        decaytypes[114, 174] = new double[,] { { 8, 1.000 } };
        decaytypes[115, 173] = new double[,] { { 8, 1.000 } };
        decaytypes[114, 175] = new double[,] { { 8, 1.000 } };
        decaytypes[116, 173] = new double[,] { { 8, 1.000 } };
        decaytypes[115, 175] = new double[,] { { 8, 1.000 } };
        decaytypes[116, 174] = new double[,] { { 8, 1.000 } };
        decaytypes[116, 175] = new double[,] { { 8, 1.000 } };
        decaytypes[116, 176] = new double[,] { { 8, 1.000 } };
        decaytypes[117, 175] = new double[,] { { 8, 0.500 }, { 19, 0.500 } };
        decaytypes[116, 177] = new double[,] { { 8, 1.000 } };
        decaytypes[118, 175] = new double[,] { { 8, 1.000 } };
        decaytypes[117, 177] = new double[,] { { 8, 1.000 } };
        decaytypes[118, 176] = new double[,] { { 8, 1.000 } };
        decaytypes[118, 177] = new double[,] { { 8, 1.000 } };


        //PREDICTED NUCLIDES START HERE
        decaytypes[82, 139] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 140] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 141] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 142] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 143] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 144] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 145] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 146] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[82, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 142] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 143] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 144] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 145] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 146] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[83, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 144] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 145] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 146] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[84, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 145] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 146] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[85, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 146] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[86, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 147] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[87, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 148] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[88, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 149] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[89, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 150] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[90, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 151] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[91, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 152] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[92, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 153] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[93, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 126] = new double[,] { { 8, 1.00000 } };
        decaytypes[94, 129] = new double[,] { { 8, 1.00000 } };
        decaytypes[94, 130] = new double[,] { { 8, 1.00000 } };
        decaytypes[94, 131] = new double[,] { { 8, 1.00000 } };
        decaytypes[94, 132] = new double[,] { { 8, 1.00000 } };
        decaytypes[94, 154] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[94, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 126] = new double[,] { { 8, 1.00000 } };
        decaytypes[95, 130] = new double[,] { { 8, 1.00000 } };
        decaytypes[95, 131] = new double[,] { { 8, 0.98379 }, { 19, 0.01621 } };
        decaytypes[95, 132] = new double[,] { { 8, 1.00000 } };
        decaytypes[95, 133] = new double[,] { { 8, 0.68100 }, { 19, 0.31900 } };
        decaytypes[95, 155] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 156] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[95, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 126] = new double[,] { { 8, 1.00000 } };
        decaytypes[96, 130] = new double[,] { { 8, 0.99095 }, { 19, 0.00905 } };
        decaytypes[96, 131] = new double[,] { { 8, 0.99101 }, { 19, 0.00899 } };
        decaytypes[96, 132] = new double[,] { { 8, 0.11000 }, { 19, 0.89000 } };
        decaytypes[96, 133] = new double[,] { { 8, 0.01351 }, { 19, 0.98649 } };
        decaytypes[96, 157] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[96, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 126] = new double[,] { { 8, 1.00000 } };
        decaytypes[97, 158] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[97, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 126] = new double[,] { { 8, 0.94009 }, { 19, 0.05991 } };
        decaytypes[98, 137] = new double[,] { { 19, 1.00000 } };
        decaytypes[98, 138] = new double[,] { { 19, 1.00000 } };
        decaytypes[98, 159] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[98, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 138] = new double[,] { { 8, 0.00146 }, { 19, 0.99854 } };
        decaytypes[99, 139] = new double[,] { { 8, 0.00941 }, { 19, 0.99059 } };
        decaytypes[99, 160] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 174] = new double[,] { { 0, 1.00000 } };
        decaytypes[99, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 138] = new double[,] { { 19, 1.00000 } };
        decaytypes[100, 139] = new double[,] { { 8, 0.01339 }, { 19, 0.98661 } };
        decaytypes[100, 140] = new double[,] { { 8, 0.25000 }, { 19, 0.75000 } };
        decaytypes[100, 161] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 162] = new double[,] { { 19, 1.00000 } };
        decaytypes[100, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 174] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 176] = new double[,] { { 0, 1.00000 } };
        decaytypes[100, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 138] = new double[,] { { 8, 0.00163 }, { 19, 0.99837 } };
        decaytypes[101, 139] = new double[,] { { 8, 0.00147 }, { 19, 0.99853 } };
        decaytypes[101, 140] = new double[,] { { 8, 0.14800 }, { 19, 0.85200 } };
        decaytypes[101, 141] = new double[,] { { 8, 0.23600 }, { 19, 0.76400 } };
        decaytypes[101, 142] = new double[,] { { 8, 0.96919 }, { 19, 0.03081 } };
        decaytypes[101, 143] = new double[,] { { 8, 0.95425 }, { 1, 0.00124 }, { 19, 0.04451 } };
        decaytypes[101, 162] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 168] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 169] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 170] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 174] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 176] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[101, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 141] = new double[,] { { 8, 0.08463 }, { 19, 0.91537 } };
        decaytypes[102, 142] = new double[,] { { 8, 0.17600 }, { 19, 0.82400 } };
        decaytypes[102, 143] = new double[,] { { 8, 0.81600 }, { 19, 0.18400 } };
        decaytypes[102, 144] = new double[,] { { 8, 0.96461 }, { 19, 0.03539 } };
        decaytypes[102, 145] = new double[,] { { 8, 0.99895 }, { 19, 0.00105 } };
        decaytypes[102, 163] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 164] = new double[,] { { 8, 0.00526 }, { 19, 0.99474 } };
        decaytypes[102, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 166] = new double[,] { { 0, 0.07909 }, { 19, 0.92091 } };
        decaytypes[102, 167] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 168] = new double[,] { { 0, 0.49700 }, { 19, 0.50300 } };
        decaytypes[102, 169] = new double[,] { { 0, 0.99895 }, { 19, 0.00105 } };
        decaytypes[102, 170] = new double[,] { { 0, 0.86300 }, { 19, 0.13700 } };
        decaytypes[102, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 174] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 176] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[102, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 142] = new double[,] { { 8, 0.03161 }, { 19, 0.96839 } };
        decaytypes[103, 143] = new double[,] { { 8, 0.03490 }, { 19, 0.96510 } };
        decaytypes[103, 144] = new double[,] { { 8, 0.77000 }, { 19, 0.23000 } };
        decaytypes[103, 145] = new double[,] { { 8, 0.70270 }, { 19, 0.29730 } };
        decaytypes[103, 146] = new double[,] { { 8, 0.97149 }, { 19, 0.02851 } };
        decaytypes[103, 147] = new double[,] { { 8, 0.96535 }, { 1, 0.00194 }, { 19, 0.03271 } };
        decaytypes[103, 164] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 165] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 166] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 167] = new double[,] { { 0, 0.99857 }, { 19, 0.00143 } };
        decaytypes[103, 168] = new double[,] { { 0, 0.98510 }, { 19, 0.01490 } };
        decaytypes[103, 169] = new double[,] { { 0, 0.97371 }, { 19, 0.02629 } };
        decaytypes[103, 170] = new double[,] { { 0, 0.98410 }, { 19, 0.01590 } };
        decaytypes[103, 171] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 172] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 174] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 176] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[103, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 143] = new double[,] { { 8, 0.00448 }, { 19, 0.99552 } };
        decaytypes[104, 144] = new double[,] { { 8, 0.01620 }, { 19, 0.98380 } };
        decaytypes[104, 145] = new double[,] { { 8, 0.23400 }, { 19, 0.76600 } };
        decaytypes[104, 146] = new double[,] { { 8, 0.25300 }, { 19, 0.74700 } };
        decaytypes[104, 147] = new double[,] { { 8, 0.83700 }, { 19, 0.16300 } };
        decaytypes[104, 148] = new double[,] { { 8, 0.97029 }, { 19, 0.02971 } };
        decaytypes[104, 165] = new double[,] { { 8, 0.29982 }, { 0, 0.68759 }, { 19, 0.01259 } };
        decaytypes[104, 166] = new double[,] { { 8, 0.15400 }, { 19, 0.84600 } };
        decaytypes[104, 167] = new double[,] { { 0, 0.94966 }, { 19, 0.05034 } };
        decaytypes[104, 168] = new double[,] { { 19, 1.00000 } };
        decaytypes[104, 169] = new double[,] { { 0, 0.37000 }, { 19, 0.63000 } };
        decaytypes[104, 170] = new double[,] { { 19, 1.00000 } };
        decaytypes[104, 171] = new double[,] { { 0, 0.97200 }, { 19, 0.02800 } };
        decaytypes[104, 172] = new double[,] { { 0, 0.03000 }, { 19, 0.97000 } };
        decaytypes[104, 173] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 174] = new double[,] { { 0, 0.89700 }, { 19, 0.10300 } };
        decaytypes[104, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 176] = new double[,] { { 0, 0.98510 }, { 19, 0.01490 } };
        decaytypes[104, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 178] = new double[,] { { 0, 0.99881 }, { 19, 0.00119 } };
        decaytypes[104, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[104, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 145] = new double[,] { { 8, 0.00126 }, { 19, 0.99874 } };
        decaytypes[105, 146] = new double[,] { { 8, 0.08246 }, { 19, 0.91754 } };
        decaytypes[105, 147] = new double[,] { { 8, 0.05991 }, { 19, 0.94009 } };
        decaytypes[105, 148] = new double[,] { { 8, 0.87387 }, { 19, 0.12613 } };
        decaytypes[105, 149] = new double[,] { { 8, 0.91864 }, { 19, 0.08136 } };
        decaytypes[105, 166] = new double[,] { { 8, 0.98351 }, { 19, 0.01649 } };
        decaytypes[105, 167] = new double[,] { { 8, 0.00174 }, { 0, 0.98357 }, { 19, 0.01469 } };
        decaytypes[105, 168] = new double[,] { { 8, 0.08200 }, { 0, 0.51000 }, { 19, 0.40800 } };
        decaytypes[105, 169] = new double[,] { { 0, 0.61600 }, { 19, 0.38400 } };
        decaytypes[105, 170] = new double[,] { { 0, 0.04962 }, { 19, 0.95038 } };
        decaytypes[105, 171] = new double[,] { { 0, 0.69400 }, { 19, 0.30600 } };
        decaytypes[105, 172] = new double[,] { { 0, 0.70300 }, { 19, 0.29700 } };
        decaytypes[105, 173] = new double[,] { { 0, 0.99430 }, { 19, 0.00570 } };
        decaytypes[105, 174] = new double[,] { { 0, 0.99813 }, { 19, 0.00187 } };
        decaytypes[105, 175] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 176] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[105, 184] = new double[,] { { 0, 1.00000 } };
        decaytypes[106, 147] = new double[,] { { 8, 0.01180 }, { 19, 0.98820 } };
        decaytypes[106, 148] = new double[,] { { 8, 0.08237 }, { 19, 0.91763 } };
        decaytypes[106, 149] = new double[,] { { 8, 0.59300 }, { 19, 0.40700 } };
        decaytypes[106, 150] = new double[,] { { 8, 0.62700 }, { 19, 0.37300 } };
        decaytypes[106, 151] = new double[,] { { 8, 0.98159 }, { 19, 0.01841 } };
        decaytypes[106, 168] = new double[,] { { 8, 0.41800 }, { 19, 0.58200 } };
        decaytypes[106, 169] = new double[,] { { 8, 0.00316 }, { 19, 0.99684 } };
        decaytypes[106, 170] = new double[,] { { 19, 1.00000 } };
        decaytypes[106, 171] = new double[,] { { 0, 0.06791 }, { 19, 0.93209 } };
        decaytypes[106, 172] = new double[,] { { 19, 1.00000 } };
        decaytypes[106, 173] = new double[,] { { 0, 0.30700 }, { 19, 0.69300 } };
        decaytypes[106, 174] = new double[,] { { 19, 1.00000 } };
        decaytypes[106, 175] = new double[,] { { 0, 0.99809 }, { 19, 0.00191 } };
        decaytypes[106, 176] = new double[,] { { 0, 0.29400 }, { 19, 0.70600 } };
        decaytypes[106, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[106, 178] = new double[,] { { 0, 0.89000 }, { 19, 0.11000 } };
        decaytypes[106, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[106, 180] = new double[,] { { 0, 0.99868 }, { 19, 0.00132 } };
        decaytypes[106, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[106, 182] = new double[,] { { 0, 0.99632 }, { 19, 0.00368 } };
        decaytypes[106, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[106, 184] = new double[,] { { 0, 0.58100 }, { 19, 0.41900 } };
        decaytypes[106, 185] = new double[,] { { 0, 0.54300 }, { 19, 0.45700 } };
        decaytypes[106, 186] = new double[,] { { 19, 1.00000 } };
        decaytypes[107, 148] = new double[,] { { 8, 0.02581 }, { 19, 0.97419 } };
        decaytypes[107, 149] = new double[,] { { 8, 0.03161 }, { 19, 0.96839 } };
        decaytypes[107, 150] = new double[,] { { 8, 0.65900 }, { 19, 0.34100 } };
        decaytypes[107, 151] = new double[,] { { 8, 0.57600 }, { 19, 0.42400 } };
        decaytypes[107, 152] = new double[,] { { 8, 0.99428 }, { 19, 0.00572 } };
        decaytypes[107, 169] = new double[,] { { 8, 0.03640 }, { 0, 0.01850 }, { 19, 0.9451 } };
        decaytypes[107, 170] = new double[,] { { 19, 1.00000 } };
        decaytypes[107, 171] = new double[,] { { 0, 0.07654 }, { 19, 0.92346 } };
        decaytypes[107, 172] = new double[,] { { 19, 1.00000 } };
        decaytypes[107, 173] = new double[,] { { 0, 0.84700 }, { 19, 0.15300 } };
        decaytypes[107, 174] = new double[,] { { 0, 0.86800 }, { 19, 0.13200 } };
        decaytypes[107, 175] = new double[,] { { 0, 0.99884 }, { 19, 0.00116 } };
        decaytypes[107, 176] = new double[,] { { 0, 0.99671 }, { 19, 0.00329 } };
        decaytypes[107, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 184] = new double[,] { { 0, 1.00000 } };
        decaytypes[107, 185] = new double[,] { { 0, 0.02749 }, { 19, 0.97251 } };
        decaytypes[107, 186] = new double[,] { { 0, 0.12500 }, { 19, 0.87500 } };
        decaytypes[108, 149] = new double[,] { { 8, 0.01210 }, { 19, 0.98790 } };
        decaytypes[108, 150] = new double[,] { { 8, 0.04852 }, { 19, 0.95148 } };
        decaytypes[108, 151] = new double[,] { { 8, 0.59900 }, { 19, 0.40100 } };
        decaytypes[108, 152] = new double[,] { { 8, 0.97049 }, { 19, 0.02951 } };
        decaytypes[108, 153] = new double[,] { { 8, 1.00000 } };
        decaytypes[108, 154] = new double[,] { { 8, 1.00000 } };
        decaytypes[108, 170] = new double[,] { { 8, 0.45800 }, { 19, 0.54200 } };
        decaytypes[108, 171] = new double[,] { { 8, 0.01530 }, { 19, 0.98470 } };
        decaytypes[108, 172] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 173] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 174] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 175] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 176] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 178] = new double[,] { { 19, 1.00000 } };
        decaytypes[108, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[108, 184] = new double[,] { { 0, 0.98960 }, { 19, 0.01040 } };
        decaytypes[108, 185] = new double[,] { { 0, 0.96990 }, { 19, 0.03010 } };
        decaytypes[108, 186] = new double[,] { { 0, 0.00202 }, { 19, 0.99798 } };
        decaytypes[108, 187] = new double[,] { { 0, 0.00369 }, { 19, 0.99631 } };
        decaytypes[109, 150] = new double[,] { { 8, 0.09591 }, { 19, 0.90409 } };
        decaytypes[109, 151] = new double[,] { { 8, 0.11700 }, { 19, 0.88300 } };
        decaytypes[109, 152] = new double[,] { { 8, 0.98420 }, { 19, 0.01580 } };
        decaytypes[109, 153] = new double[,] { { 8, 0.99867 }, { 19, 0.00133 } };
        decaytypes[109, 154] = new double[,] { { 8, 1.00000 } };
        decaytypes[109, 155] = new double[,] { { 8, 1.00000 } };
        decaytypes[109, 171] = new double[,] { { 8, 0.01751 }, { 19, 0.98249 } };
        decaytypes[109, 172] = new double[,] { { 8, 0.01849 }, { 19, 0.98151 } };
        decaytypes[109, 173] = new double[,] { { 0, 0.18800 }, { 19, 0.81200 } };
        decaytypes[109, 174] = new double[,] { { 19, 1.00000 } };
        decaytypes[109, 175] = new double[,] { { 0, 0.99655 }, { 19, 0.00345 } };
        decaytypes[109, 176] = new double[,] { { 0, 0.99144 }, { 19, 0.00856 } };
        decaytypes[109, 177] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 178] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 184] = new double[,] { { 0, 1.00000 } };
        decaytypes[109, 185] = new double[,] { { 0, 0.94091 }, { 19, 0.05909 } };
        decaytypes[109, 186] = new double[,] { { 0, 0.35000 }, { 19, 0.65000 } };
        decaytypes[109, 187] = new double[,] { { 0, 0.00357 }, { 19, 0.99643 } };
        decaytypes[109, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[110, 151] = new double[,] { { 8, 0.07863 }, { 19, 0.92137 } };
        decaytypes[110, 152] = new double[,] { { 8, 0.64300 }, { 19, 0.35700 } };
        decaytypes[110, 153] = new double[,] { { 8, 0.99299 }, { 19, 0.00701 } };
        decaytypes[110, 155] = new double[,] { { 8, 1.00000 } };
        decaytypes[110, 156] = new double[,] { { 8, 0.99779 }, { 19, 0.00221 } };
        decaytypes[110, 172] = new double[,] { { 19, 1.00000 } };
        decaytypes[110, 173] = new double[,] { { 8, 0.01300 }, { 19, 0.98700 } };
        decaytypes[110, 174] = new double[,] { { 8, 0.02839 }, { 19, 0.97161 } };
        decaytypes[110, 175] = new double[,] { { 8, 0.75200 }, { 19, 0.24800 } };
        decaytypes[110, 176] = new double[,] { { 8, 0.83500 }, { 19, 0.16500 } };
        decaytypes[110, 177] = new double[,] { { 8, 0.99879 }, { 19, 0.00121 } };
        decaytypes[110, 178] = new double[,] { { 8, 0.85500 }, { 19, 0.14500 } };
        decaytypes[110, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[110, 180] = new double[,] { { 8, 0.03639 }, { 19, 0.96361 } };
        decaytypes[110, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[110, 182] = new double[,] { { 0, 0.97310 }, { 19, 0.02690 } };
        decaytypes[110, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[110, 184] = new double[,] { { 0, 0.92363 }, { 19, 0.07637 } };
        decaytypes[110, 185] = new double[,] { { 0, 0.97710 }, { 19, 0.02290 } };
        decaytypes[110, 186] = new double[,] { { 0, 0.00485 }, { 19, 0.99515 } };
        decaytypes[110, 187] = new double[,] { { 0, 0.00760 }, { 19, 0.99240 } };
        decaytypes[110, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[110, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[111, 155] = new double[,] { { 8, 0.71400 }, { 19, 0.28600 } };
        decaytypes[111, 156] = new double[,] { { 8, 0.97361 }, { 19, 0.02639 } };
        decaytypes[111, 157] = new double[,] { { 8, 0.73400 }, { 19, 0.26600 } };
        decaytypes[111, 158] = new double[,] { { 8, 0.98180 }, { 19, 0.01820 } };
        decaytypes[111, 159] = new double[,] { { 8, 0.98951 }, { 19, 0.01049 } };
        decaytypes[111, 160] = new double[,] { { 8, 1.00000 } };
        decaytypes[111, 173] = new double[,] { { 8, 0.12000 }, { 1, 0.76300 }, { 19, 0.11700 } };
        decaytypes[111, 174] = new double[,] { { 8, 0.97051 }, { 19, 0.02949 } };
        decaytypes[111, 175] = new double[,] { { 8, 0.39030 }, { 1, 0.60546 }, { 19, 0.00424 } };
        decaytypes[111, 176] = new double[,] { { 8, 1.00000 } };
        decaytypes[111, 177] = new double[,] { { 8, 0.00400 }, { 0, 0.99600 } };
        decaytypes[111, 178] = new double[,] { { 8, 0.98910 }, { 19, 0.01090 } };
        decaytypes[111, 179] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 180] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 182] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 184] = new double[,] { { 0, 1.00000 } };
        decaytypes[111, 185] = new double[,] { { 0, 0.98630 }, { 19, 0.01370 } };
        decaytypes[111, 186] = new double[,] { { 0, 0.63500 }, { 19, 0.36500 } };
        decaytypes[111, 187] = new double[,] { { 0, 0.00848 }, { 19, 0.99152 } };
        decaytypes[111, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[111, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[112, 157] = new double[,] { { 8, 0.31600 }, { 19, 0.68400 } };
        decaytypes[112, 158] = new double[,] { { 8, 0.49300 }, { 19, 0.50700 } };
        decaytypes[112, 159] = new double[,] { { 8, 0.94728 }, { 19, 0.05272 } };
        decaytypes[112, 160] = new double[,] { { 8, 0.98390 }, { 19, 0.01610 } };
        decaytypes[112, 161] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 162] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 163] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 174] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 175] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 176] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 177] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[112, 179] = new double[,] { { 8, 0.99505 }, { 19, 0.00495 } };
        decaytypes[112, 180] = new double[,] { { 8, 0.99691 }, { 19, 0.00309 } };
        decaytypes[112, 181] = new double[,] { { 8, 0.99858 }, { 19, 0.00142 } };
        decaytypes[112, 182] = new double[,] { { 8, 0.93219 }, { 19, 0.06781 } };
        decaytypes[112, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[112, 184] = new double[,] { { 8, 0.16500 }, { 19, 0.83500 } };
        decaytypes[112, 185] = new double[,] { { 8, 0.00374 }, { 0, 0.99049 }, { 19, 0.00577 } };
        decaytypes[112, 186] = new double[,] { { 8, 0.00579 }, { 19, 0.99421 } };
        decaytypes[112, 187] = new double[,] { { 0, 0.01300 }, { 19, 0.98700 } };
        decaytypes[112, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[112, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[113, 159] = new double[,] { { 8, 0.26800 }, { 19, 0.73200 } };
        decaytypes[113, 160] = new double[,] { { 8, 0.96081 }, { 19, 0.03919 } };
        decaytypes[113, 161] = new double[,] { { 8, 0.97471 }, { 19, 0.02529 } };
        decaytypes[113, 162] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 163] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 164] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 175] = new double[,] { { 8, 0.79000 }, { 1, 0.21000 } };
        decaytypes[113, 176] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 177] = new double[,] { { 8, 0.54500 }, { 1, 0.45500 } };
        decaytypes[113, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 179] = new double[,] { { 8, 0.00343 }, { 0, 0.97278 }, { 1, 0.02379 } };
        decaytypes[113, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[113, 181] = new double[,] { { 0, 1.00000 } };
        decaytypes[113, 182] = new double[,] { { 8, 0.19100 }, { 0, 0.80900 } };
        decaytypes[113, 183] = new double[,] { { 0, 1.00000 } };
        decaytypes[113, 184] = new double[,] { { 8, 0.00873 }, { 0, 0.99127 } };
        decaytypes[113, 185] = new double[,] { { 8, 0.04708 }, { 0, 0.95066 }, { 19, 0.00226 } };
        decaytypes[113, 186] = new double[,] { { 8, 0.21800 }, { 0, 0.60200 }, { 19, 0.18000 } };
        decaytypes[113, 187] = new double[,] { { 0, 0.02480 }, { 19, 0.97520 } };
        decaytypes[113, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[113, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[113, 190] = new double[,] { { 19, 1.00000 } };
        decaytypes[114, 161] = new double[,] { { 8, 0.85400 }, { 19, 0.14600 } };
        decaytypes[114, 162] = new double[,] { { 8, 0.95310 }, { 19, 0.04690 } };
        decaytypes[114, 163] = new double[,] { { 8, 0.99861 }, { 19, 0.00139 } };
        decaytypes[114, 165] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 166] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 167] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 168] = new double[,] { { 8, 0.99105 }, { 19, 0.00895 } };
        decaytypes[114, 169] = new double[,] { { 8, 0.98997 }, { 1, 0.00608 }, { 19, 0.00395 } };
        decaytypes[114, 176] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 177] = new double[,] { { 8, 0.99558 }, { 1, 0.00442 } };
        decaytypes[114, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 179] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 184] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 185] = new double[,] { { 8, 1.00000 } };
        decaytypes[114, 186] = new double[,] { { 8, 0.98180 }, { 19, 0.01820 } };
        decaytypes[114, 187] = new double[,] { { 8, 0.16410 }, { 0, 0.00343 }, { 19, 0.83247 } };
        decaytypes[114, 188] = new double[,] { { 19, 1.00000 } };
        decaytypes[114, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[114, 190] = new double[,] { { 19, 1.00000 } };
        decaytypes[115, 162] = new double[,] { { 8, 0.82900 }, { 19, 0.17100 } };
        decaytypes[115, 163] = new double[,] { { 8, 0.90909 }, { 19, 0.09091 } };
        decaytypes[115, 164] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 165] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 166] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 167] = new double[,] { { 8, 0.99011 }, { 19, 0.00989 } };
        decaytypes[115, 168] = new double[,] { { 8, 0.99594 }, { 1, 0.00107 }, { 19, 0.00299 } };
        decaytypes[115, 169] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 170] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 171] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 177] = new double[,] { { 8, 0.97300 }, { 1, 0.02700 } };
        decaytypes[115, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 179] = new double[,] { { 8, 0.96171 }, { 1, 0.03829 } };
        decaytypes[115, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 184] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 185] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 186] = new double[,] { { 8, 1.00000 } };
        decaytypes[115, 187] = new double[,] { { 8, 0.14300 }, { 19, 0.85700 } };
        decaytypes[115, 188] = new double[,] { { 8, 0.06572 }, { 19, 0.93428 } };
        decaytypes[115, 189] = new double[,] { { 19, 1.00000 } };
        decaytypes[115, 190] = new double[,] { { 19, 1.00000 } };
        decaytypes[116, 163] = new double[,] { { 8, 0.78700 }, { 19, 0.21300 } };
        decaytypes[116, 164] = new double[,] { { 8, 0.96539 }, { 19, 0.03461 } };
        decaytypes[116, 165] = new double[,] { { 8, 0.99331 }, { 19, 0.00669 } };
        decaytypes[116, 166] = new double[,] { { 8, 0.96739 }, { 19, 0.03261 } };
        decaytypes[116, 167] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 168] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 169] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 170] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 171] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 172] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 179] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 184] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 185] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 186] = new double[,] { { 8, 1.00000 } };
        decaytypes[116, 187] = new double[,] { { 8, 0.84200 }, { 19, 0.15800 } };
        decaytypes[116, 188] = new double[,] { { 8, 0.12400 }, { 19, 0.87600 } };
        decaytypes[117, 164] = new double[,] { { 8, 0.87300 }, { 19, 0.12700 } };
        decaytypes[117, 165] = new double[,] { { 8, 0.63300 }, { 19, 0.36700 } };
        decaytypes[117, 166] = new double[,] { { 8, 0.99873 }, { 19, 0.00127 } };
        decaytypes[117, 167] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 168] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 169] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 170] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 171] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 172] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 173] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 179] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 184] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 185] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 186] = new double[,] { { 8, 1.00000 } };
        decaytypes[117, 187] = new double[,] { { 8, 0.24700 }, { 19, 0.75300 } };
        decaytypes[117, 188] = new double[,] { { 8, 0.14900 }, { 19, 0.85100 } };
        decaytypes[118, 166] = new double[,] { { 8, 0.91182 }, { 19, 0.08818 } };
        decaytypes[118, 167] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 168] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 169] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 170] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 171] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 172] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 173] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 174] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 179] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 184] = new double[,] { { 8, 1.00000 } };
        decaytypes[118, 185] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 168] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 169] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 170] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 171] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 172] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 173] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 174] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 175] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 176] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 177] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 178] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 179] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 180] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 181] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 182] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 183] = new double[,] { { 8, 1.00000 } };
        decaytypes[119, 184] = new double[,] { { 8, 1.00000 } };
        for (int i = 0; i <= MAXP; i++)
        {
            for (int j = 0; j <= MAXN; j++) {
                if (decaytypes[i, j] != null)
                {
                    double sum = 0;
                    for (int k = 0; k < decaytypes[i, j].GetLength(0); k++)
                    {
                        sum += decaytypes[i, j][k, 1];
                    }
                    if (Math.Abs(sum-1) > 1e-6 && halflives[i,j] != -1)
                    {
                        Debug.Log(string.Format("Decay entry {0},{1} does not sum to 1, sums to {2}", i, j, sum));
                    }
                }
            }
        }

        watch.Stop();
        halflifemap = new Texture2D(MAXP + 200, MAXN + 200);
        for (int i = 0; i < halflifemap.height; i++)
        {
            for (int j = 0; j < halflifemap.width; j++)
            {
                int index = mapColors.Length - 2;
                double life = GetHalfLife(j - 100, i - 100);

                if (life == 0)
                {
                    index = mapColors.Length - 1;
                }
                else if (life != -1) {
                    float logLife = (float)(Math.Log10(life));
                    index = (int)(Mathf.Clamp(logLife + 4.0f,0, mapColors.Length - 3));
                }
                halflifemap.SetPixel(j, i, mapColors[index]);
            }
        }
        halflifemap.Apply();
        System.IO.File.WriteAllBytes(Application.dataPath + "/halflifemap.png", halflifemap.EncodeToPNG());
        Debug.Log(string.Format("Took {0} ms to initialize constants.\n", watch.ElapsedMilliseconds));
    }

    public static double GetHalfLife(int Z, int N)
    {
        if (Z >= 0 && Z <= MAXP && N >= 0 && N <= MAXN)
        {
            //Debug.Log(String.Format("Half life for {0},{1}, checking {2}", Z, N, halflives));
            if (halflives == null)
            {
                Debug.Log(string.Format("Warning: could not find half life for Z={0}, N={0}", Z, N));
            }
            return halflives[Z, N];
        }
        else
        {
            return 0;
        }
    }

    public static double GetRadius(int Z)
    {
        if (Z >= 0 && Z <= MAXP)
        {
            return radii[Z];
        }
        else
        {
            return radii[119];
        }
    }



    public static double[,] GetDecayTypes(int Z, int N)
    {
        if (Z + N == 0)
        {
            return new double[,] { { 0, 0 } }; //No decays for A=0
        }
        if (Z >= 0 && Z <= MAXP && N >= 0 && N <= MAXN && decaytypes[Z, N] != null)
        {
            return decaytypes[Z, N]; //observed decay methods
        }
        else if (Z + N > 200)
        {
            return new double[,] { { 19, 1 } }; //100% SF
        }
        else if (N > 3 * Z || Z < 20) // 100% neutron emission
        {
            return new double[,] { {2, 1 } };
        }
        else if (N > 0.6223 * Math.Pow(Z, 1.2003)) //too many neutrons
        {
            return new double[,] { { 0, 1 } }; //100% B-
        }
        else //too few neutrons
        {
            if (N + Z > 140)
            {
                return new double[,] { { 8, 1 } }; //100% alpha
            }
            return new double[,] { { 3, 1 } }; //100% proton emission
        }
    }

    public static bool ShouldDecay(int Z, int N, double timescale)
    {
        double t = Time.deltaTime * timescale;
        double halflife = GetHalfLife(Z, N);
        if (halflife == 0)
        {
            return true;
        }
        else if (halflife == -1)
        {
            return false;
        }
        else
        {
            if (UnityEngine.Random.value > Math.Pow(2, -t / halflife))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static string GetFormattedLife(int Z, int N)
    {
        double life = GetHalfLife(Z, N);
        if (life == -1)
        {
            return "stable";
        }
        else if (life == 0)
        {
            return "< 1 µs";
        }
        else if (life > 315569520000)
        {
            return (life / 31556952).ToString("G2") + " yr";
        }
        else if (life > 31556952)
        {
            return (life / 31556952).ToString("F") + " yr";
        }
        else if (life > 86400)
        {
            return (life / 86400).ToString("F") + " d";
        }
        else if (life > 3600)
        {
            return (life / 3600).ToString("F") + " h";
        }
        else if (life > 1)
        {
            return life.ToString("F") + " s";
        }
        else if (life > 1e-3)
        {
            return (life * 1000).ToString("F") + " ms";
        }
        else
        {
            return (life * 1e6).ToString("F") + " µs";
        }
    }

    public static string GetAbundance(int Z)
    {
        if (abundances[Z] > 1e-2)
        {
            return (abundances[Z] * 100).ToString("F") + " %";
        }
        else if (abundances[Z] > 1e-8)
        {
            return ((int)(abundances[Z] * 1e9)).ToString() + " ppb";
        }
        else if (abundances[Z] > 0)
        {
            return (abundances[Z] * 1e9).ToString("F4") + " ppb";
        }
        else if (Z > 100)
        {
            return "0";
        }
        else
        {
            return "< 3E-09 ppb";
        }

    }

    public static Color HalfLifeColor(int Z, int N, int E)
    {
        double life = GetHalfLife(Z, N);
        int index = mapColors.Length - 2;
        if (life == 0)
        {
            index = mapColors.Length - 1;
        }
        else if (life != -1)
        {
            float logLife = (float)(Math.Log10(life));
            index = (int)(Mathf.Clamp(logLife + 4.0f, 0, mapColors.Length - 3));
        }

        return mapColors[index];
    }

    public static Color MainHalfLifeColor(int Z, int N, int E)
    {
        double life = GetHalfLife(Z, N);
        if (life == -1)
        {
            return new Color(1, 1, 1);
        }
        else if (life > 3600f * 24 * 365) //1y
        {
            return new Color(0, 1, 0);
        }
        else if (life > 3600f * 24) //1d
        {
            return new Color(0.5f, 1, 0);
        }
        else if (life > 3600f) //1h
        {
            return new Color(1, 1, 0);
        }
        else if (life > 60f) //1m
        {
            return new Color(1, 0.5f, 0);
        }
        else
        {
            return new Color(1, 0.5f, 0);
        }    
    }

    public static Color ShellColor(int Z, int N, int E)
    {
        if (Z == 0)
        {
            return new Color(1, 1, 1);
        }
        switch (subshells[Z][1]) {
            case 's':
                return shellLabelColors[0];
            case 'p':
                return shellLabelColors[1];
            case 'd':
                return shellLabelColors[2];
            case 'f':
                return shellLabelColors[3];
            case 'g':
                return shellLabelColors[4];
            default:
                return new Color(1,1,1);
        }
    }

    public static Color GroupColor(int Z, int N, int E)
    {
        return typeColors[elementType2[Z]];
    }

    public static Color DiscoveryColor(int Z, int N, int E)
    {
        if (discoveryYears[Z].Equals("Unconfirmed")) {
            return new Color(0, 0, 0);
        }
        if (discoveryYears[Z].Length > 4)
        {
            return new Color(1, 0, 0);
        }
        int year;
        if (Z + N > 0) {
            year = int.Parse(discoveryYears[Z]);
        }
        else if (E == 1) {
            year = int.Parse(electronDiscoveryYear);
        }
        else {
            year = int.Parse(positronDiscoveryYear);
        }

        if (year < 1700)
        {
            return new Color(1, 1, 0);
        }
        else if (year < 1869)
        {
            return new Color(0, 1, 0);
        }
        else if (year < 1940)
        {
            return new Color(0, 1, 1);
        }
        else if (year < 1990)
        {
            return new Color(0.4f, 0.4f, 1);
        }
        else
        {
            return new Color(1, 0, 1);
        }
    }
}
