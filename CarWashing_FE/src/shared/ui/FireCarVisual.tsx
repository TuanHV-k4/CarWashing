import { motion, useReducedMotion } from 'motion/react'

export function FireCarVisual({ compact = false }: { compact?: boolean }) {
  const shouldReduceMotion = useReducedMotion()
  return <div className={`fire-car${compact ? ' compact' : ''}`} role="img" aria-label="Xe thể thao với hiệu ứng lửa trang trí">
    <div className="fire-car-bg" />
    <motion.svg className="car-visual" viewBox="0 0 700 390" fill="none" xmlns="http://www.w3.org/2000/svg" animate={shouldReduceMotion ? undefined : { y: [0, -5, 0] }} transition={{ duration: 3.2, repeat: Infinity, ease: 'easeInOut' }}>
      <g className="flame"><path d="M99 247C42 226 30 169 86 107C86 151 112 156 127 180C119 142 150 100 195 82C177 137 228 163 206 226L99 247Z" fill="url(#fire)" /><path d="M511 244C569 213 584 158 532 98C533 139 506 157 492 182C500 140 469 103 430 83C445 136 396 168 415 229L511 244Z" fill="url(#fire)" /></g>
      <path d="M101 246C126 207 175 180 256 173L389 174C473 180 548 205 590 251L615 286H72L101 246Z" fill="#0E2E50" /><path d="M192 183L260 113H386L462 184H192Z" fill="#9FE9E4" fillOpacity=".93" /><path d="M266 122H381L432 178H219L266 122Z" fill="#19476E" /><path d="M218 186H467" stroke="#C9FFFF" strokeWidth="7" strokeLinecap="round" /><path d="M88 263H605L583 300H107L88 263Z" fill="#146B78" /><path d="M105 281H582" stroke="#F4FFFF" strokeWidth="5" strokeLinecap="round" opacity=".75" />
      <circle cx="186" cy="291" r="55" fill="#0B1727" /><circle cx="186" cy="291" r="34" fill="#AEEDEC" /><circle cx="186" cy="291" r="16" fill="#12345A" /><circle cx="514" cy="291" r="55" fill="#0B1727" /><circle cx="514" cy="291" r="34" fill="#AEEDEC" /><circle cx="514" cy="291" r="16" fill="#12345A" />
      <path d="M102 253H158L143 269H96L102 253ZM598 253H539L554 269H604L598 253Z" fill="#F9CC73" /><defs><linearGradient id="fire" x1="76" y1="105" x2="196" y2="247" gradientUnits="userSpaceOnUse"><stop stopColor="#FFF1A2" /><stop offset=".45" stopColor="#FF9C35" /><stop offset="1" stopColor="#F04F2D" /></linearGradient></defs>
    </motion.svg>
  </div>
}
