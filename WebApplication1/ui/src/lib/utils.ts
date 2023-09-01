import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function tw(...input: ClassValue[]) {
	return twMerge(clsx(input));
}
