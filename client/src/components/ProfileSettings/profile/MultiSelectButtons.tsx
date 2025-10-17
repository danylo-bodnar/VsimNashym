type MultiSelectButtonsProps = {
  label: string
  options: readonly string[]
  selected: string[]
  onToggle: (item: string) => void
}

export default function MultiSelectButtons({
  label,
  options,
  selected,
  onToggle,
}: MultiSelectButtonsProps) {
  return (
    <div className="space-y-2">
      <label className="block text-xs uppercase tracking-wider text-gray-500 font-medium">
        {label}
      </label>
      <div className="flex flex-wrap gap-2">
        {options.map((option) => (
          <button
            key={option}
            type="button"
            onClick={() => onToggle(option)}
            className={`px-4 py-2 border-2 text-sm font-medium transition-colors ${
              selected.includes(option)
                ? 'border-black bg-black text-white'
                : 'border-gray-300 text-black hover:border-black'
            }`}
          >
            {option}
          </button>
        ))}
      </div>
    </div>
  )
}
