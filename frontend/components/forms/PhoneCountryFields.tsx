"use client";

import "react-phone-number-input/style.css";
import PhoneNumberInput from "react-phone-number-input/input";
import {
  getCountries,
  getCountryCallingCode,
  type Country,
} from "react-phone-number-input";
import en from "react-phone-number-input/locale/en";

interface PhoneCountryFieldsProps {
  country: Country;
  phoneNumber: string | undefined;
  onCountryChange: (country: Country) => void;
  onPhoneChange: (value: string | undefined) => void;
}

export function PhoneCountryFields({
  country,
  phoneNumber,
  onCountryChange,
  onPhoneChange,
}: PhoneCountryFieldsProps) {
  const countries = getCountries();

  return (
    <div className="grid gap-5 sm:grid-cols-[0.9fr_1.1fr]">
      {/* Country */}
      <div>
        <label
          htmlFor="country"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Country <span className="text-[#F5821F]">*</span>
        </label>

        <select
          id="country"
          name="country"
          value={country}
          onChange={(event) =>
            onCountryChange(event.target.value as Country)
          }
          className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none transition-colors focus:border-[#3E8F96]"
        >
          {countries.map((countryCode) => (
            <option key={countryCode} value={countryCode}>
              {en[countryCode]} (+{getCountryCallingCode(countryCode)})
            </option>
          ))}
        </select>
      </div>

      {/* Phone */}
      <div>
        <label
          htmlFor="phone-number"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Phone Number
        </label>

        <div className="flex h-11 items-center rounded-lg border border-[#dfe4e3] bg-white px-3 focus-within:border-[#3E8F96]">
          <span className="mr-2 shrink-0 text-sm text-[#595959]">
            +{getCountryCallingCode(country)}
          </span>

          <PhoneNumberInput
            country={country}
            value={phoneNumber}
            onChange={onPhoneChange}
            placeholder="Enter phone number"
            id="phone-number"
            name="phoneNumber"
            className="h-full w-full border-0 bg-transparent p-0 text-sm text-[#1B2A4A] outline-none placeholder:text-[#999]"
          />
        </div>
      </div>
    </div>
  );
}